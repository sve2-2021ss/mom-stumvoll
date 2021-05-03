# Device Management with RabbitMQ

The goal of this exercise is to implement a asynchronous service using a message broker. The application will focus on routing via topics, producer confirmation and multiple implementation languages.

## Use Case

The implemented application provides a service which allows to monitor resource usage and system events of devices like servers or IoT devices. The application consists of the following three components. How they work in detail will be discussed later.

- The `Agent` which is installed on a device to gather monitoring information and share them with the broker.
- The message broker.
- The `Watchdog` which subscribes himself to certain (or all) information the `Agents` provide and sends notifications when certain thresholds are exceeded or certain events occur. (e.g. CPU load is above a certain level)

For this exercise the `Agent` does not actually gather system information, instead it one or more devices and generates random data for each of them.

## Application

The following image gives a basic overview of the whole System. How these components work and how they interact with each other will be explained in the following sections.

![Architecture](doc/architecture.png)

### Data Exchange Format

Since producer and consumer are implemented in different languages, it is necessary to exchange the data in a language agnostic form. To solve this issue, the message format `protobuf` was chosen. This format is extremely interesting, not only because its language agnostic but also because messages can be serialized to a binary format. This makes it extremely efficient when sending it over the network.

### Broker

For the broker RabbitMQ was chosen, because of its easy setup. For this application it is started using docker compose.

### Agent

As already outlined in [Use Case](#use-case) the `Agent's` job is it to gather monitoring information of a device and publish them to the broker. This application was implemented in C# and uses the `RabbitMQ.Client` library to communicate with the broker. The `protobuf-net` library is also used to transform objects to the `protobuf` format.

The following sections describe how the `Agent` is structured and how he communicates with the broker.

#### Architecture

The `Agent` basically consists of three components as already seen in the image in [Application](#application):

- multiple `SystemValueSources`
- DeviceWatcher
- SystemValueApi

These will be described in this section.

##### SystemValueSource

The `SystemValueSources` build the base of the `Agent`. They provide the application with the system values of the device (For this application the values are randomly generated). Each `SystemValueSource` must implement the following interface. It defines an event which is fired when a new value is available.

```C#
public interface ISystemValueSource<out T> where T : ISystemValue
{
    event Action<T> NewSystemValue;
}
```

For this application three sources for CPU, RAM and service events have been implemented. What information is included in each `SystemValue` can be seen below. Each `SystemValue` also contains the identifier of the device it was sent from. However this is set by the `DeviceWatcher` and not by each `SystemValueSource`.

- CPU
  - Load percentage
  - Temperature per core
  - power draw
- RAM
  - Total Mb
  - Used Mb
  - Memory Clock
- Service event
  - Executable path
  - Event type (Either started or stopped)

##### DeviceWatcher

The `DeviceWatcher` is the bridge between the `SystemValueSources` and the `SystemValueApi`. He is registered to all `SystemValueSources` and forwards the messages to the `SystemValueApi`.

##### SystemValueApi

The `SystemValueApi` abstracts the RabbitMQ implementation from the rest of the application. It provides a single `Publish` method which can be used by the application to publish `SystemValues` to the broker. How this component communicates with the broker is described in the next section.

#### Broker Communication

The following sections describe the aspects of the communication with the broker.

##### Topic Routing

The communication from the producer to the broker is realized through a topic exchange. This allows the consumer to decide which messages he wants to receive.

The tree in the following image describes the concrete routing keys to which the `Agent` publishes his messages. Each arrow represents a dot in the routing key. For example if the consumer want to receive all metrics from `device-1` he would use the routing key `device-1.metrics.*`. This structure makes it easy for the consumer to consume all metrics or all events.

![RoutingKeys](doc/routing-keys.png)

##### Confirmation

As already mentioned in [Architecture](#architecture) the communication with the broker is abstracted through the `SystemValueApi`. This class provides a single `Publish` method which can be used to send a message to the broker. When calling this method the caller can also define if he wants, that the message should be acknowledged or not.

For the context of this application this is very useful since the sent messages can be separated in two categories. `Metrics` which get sent regularly to the broker and `events` which might only occur once. For the first it doesn't matter if one of those messages get lost, however for the latter it is critical that the broker receives and handles those messages.

How the confirmation process works and how it is implemented in this application is described in the following two sections.

###### Basics

When sending messages to a RabbitMQ broker it is possible to request a response if the broker accepted or declined a message. For this the channel on which the message is sent must be put in the confirm mode. When this is done, both the publisher and the broker will count the messages starting from one. When the broker acks or nacks a message he will send the number of the message back to the producer on the same channel. The broker can additionally send a `multiple` flag which indicates that all messages till the given number have been handled or declined.

###### Implementation

To implement this acknowledgement behaviour the `SystemValueApi` needs two separate channels. One in confirm mode and one in the default mode. To do this in C# the `ConfirmSelect` needs to be called on the channel object. The C# RabbitMQ library allows to listen to the confirmation responses through the two events `BasicAcks` and `BasicNacks`. Both events contain the message number as well as the multiple flag.

Every time the `SystemValueApi` send a message which needs to get confirmed it stores this message in a `ConcurrentDictionary`. The key is the message number and the value is the `SystemValue`. The next message number can be requested by calling `NextPublishSeqNo` on the channel object. Whenever `BasicAcks` or `BasicNacks` fires an event the message in question can be retrieved from the dictionary. In case the message was nacked it will be resent.

The important implementation steps can be seen below. The full source code can be seen in `Agent/Agent.Core/SystemValueApi/SystemValueApi.cs`

```C#
/* Setup delegates */
_confirmChannel.BasicAcks += (_, args) => HandleConfirmation(args.DeliveryTag, args.Multiple, false);
_confirmChannel.BasicNacks += (_, args) => HandleConfirmation(args.DeliveryTag, args.Multiple, true);

/* HandleConfirmation method */

private void HandleConfirmation(ulong tag, bool multiple, bool nack)
{
    var resume = true;
    while (resume && _ackMap.ContainsKey(tag) && _ackMap.TryRemove(tag, out var queued))
    {
        if (nack)
        {
            Publish(queued, true);
        }

        resume = multiple;
        tag--;
    }
}

/* Storing message */
_ackMap.TryAdd(_confirmChannel.NextPublishSeqNo, payload);
```

### Watchdog

As described in [Use Case](#use-case) the `Watchdogs` purpose is to consume `SystemValues` and notify the user if certain conditions are met. For example if a value is above a certain threshold. This application is implemented using Kotlin and it uses the `kotlinx-serialization-protobuf` library to decode the `protobuf` messages. Since there is no dedicated Kotlin RabbitMQ library it uses the Java `amqp-client` library.

How the `Watchdog` is structured and how he communicates with the broker will be described below.

#### Architecture

#### Broker Communication

Same as for the `Agent` the communication with RabbitMQ is abstracted through a `SystemValueApi`.
However the communication from the broker to the `Watchdog` is much simpler than from the `Agent` to the broker. The only steps necessary are:

1. creating a channel
2. declare a queue for the channel (by calling the parameterless method `queueDeclare` a exclusive, non-durable autodelete queue with a generated name is created)
3. bind the queue to all user defined routing keys.

After that the consumer can start listening by calling `basicConsume` on the channel. This method takes the name of the queue, the auto acknowledge flag and a deliver and shutdown callback. The auto acknowledge flag tells the broker if he can consider messages acknowledged as soon as they are delivered. This was set to `false` in the `Watchdog` implementation. Instead all messages are acknowledged manually. This can be seen in the snippet below. At first the protobuf message is decoded using `decode` and then passed to the higher ordered function `onValue`. As soon as the `SystemValue` has been processed the `channel.basicAck` is called to acknowledge the message. If an exception is thrown the message is nacked and the requeue flag (third parameter of `basicNack`) is set to `false`.

```Kotlin
fun startConsume(onValue: (SystemValue) -> Unit) {
        val deliverCallback = DeliverCallback { _, delivery ->
            try {
                decode(delivery.envelope.routingKey, delivery.body)?.let {
                    onValue(it)
                }
                channel.basicAck(delivery.envelope.deliveryTag, false)
            } catch (e: Exception) {
                channel.basicNack(delivery.envelope.deliveryTag, false, false)
            }
        }

        channel.basicConsume(queueName, false, deliverCallback) { _, sig ->
            eprintln("Shutdown: ${sig.message}")
        }
    }
```

## Conclusion