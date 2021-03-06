package api

import Config
import com.rabbitmq.client.Channel
import com.rabbitmq.client.Connection
import com.rabbitmq.client.DeliverCallback
import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.decodeFromByteArray
import kotlinx.serialization.protobuf.ProtoBuf
import util.eprintln

@ExperimentalSerializationApi
class SystemValueApi(config: Config, connection: Connection) {
    private val channel: Channel = connection.createChannel()
    private val queueName: String = channel.queueDeclare().queue

    init {
        config.routingKeys.forEach {
            channel.queueBind(queueName, config.exchange, it)
        }

    }

    private inline fun <reified T> ByteArray.decodeProto(): T =
        ProtoBuf.decodeFromByteArray(this)

    private fun decode(routingKey: String, data: ByteArray): SystemValue? = when {
        routingKey.contains(Regex(""".*\.metrics\.cpu""")) -> data.decodeProto<Cpu>()
        routingKey.contains(Regex(""".*\.metrics\.ram""")) -> data.decodeProto<Ram>()
        routingKey.contains(Regex(""".*\.events.service.(started|stopped)""")) -> data.decodeProto<ServiceEvent>()
        else -> null
    }

    fun startConsume(onValue: (SystemValue) -> Unit) {
        val deliverCallback = DeliverCallback { _, delivery ->
            try {
                decode(delivery.envelope.routingKey, delivery.body)?.let {
                    onValue(it)
                }
                channel.basicAck(delivery.envelope.deliveryTag, false)
            } catch (_: MessageProcessingException) {
                channel.basicNack(delivery.envelope.deliveryTag, false, false)
            } catch (_: Exception) {
                channel.basicNack(delivery.envelope.deliveryTag, false, true)
            }
        }

        channel.basicConsume(queueName, false, deliverCallback) { _, sig ->
            eprintln("Shutdown: ${sig.message}")
        }
    }
}