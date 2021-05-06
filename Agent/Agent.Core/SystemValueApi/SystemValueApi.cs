using System;
using System.Collections.Concurrent;
using System.IO;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;
using RabbitMQ.Client;

namespace Agent.Core.SystemValueApi
{
    public sealed class SystemValueApi : ISystemValueApi
    {
        private readonly string _exchangeName;
        private readonly IModel _defaultChannel;
        private readonly IModel _confirmChannel;
        private readonly object _lockObject = new();
        private readonly ConcurrentDictionary<ulong, ISystemValue> _ackMap = new();

        public SystemValueApi(string exchangeName, IConnection connection)
        {
            _exchangeName = exchangeName;
            _defaultChannel = connection.CreateModel();
            _defaultChannel.ExchangeDeclare(exchangeName, "topic", true);

            _confirmChannel = connection.CreateModel();
            _confirmChannel.ExchangeDeclare(exchangeName, "topic", true);
            _confirmChannel.ConfirmSelect();
            _confirmChannel.BasicAcks += (_, args) => HandleConfirmation(args.DeliveryTag, args.Multiple, false);
            _confirmChannel.BasicNacks += (_, args) => HandleConfirmation(args.DeliveryTag, args.Multiple, true);
        }

        private void HandleConfirmation(ulong tag, bool multiple, bool nack)
        {
            var resume = true;
            while (resume && _ackMap.ContainsKey(tag) && _ackMap.TryRemove(tag, out var stored))
            {
                if (nack)
                {
                    Publish(stored, true);
                }

                resume = multiple;
                tag--;
            }
        }

        private static string GetKeyForType(ISystemValue systemValue) =>
            systemValue switch
            {
                Cpu => "metrics.cpu",
                Ram => "metrics.ram",
                ServiceEvent serviceEvent => GetKeyForServiceEvent(serviceEvent),
                _ => throw new ArgumentOutOfRangeException(nameof(systemValue))
            };

        private static string GetKeyForServiceEvent(ServiceEvent serviceEvent) =>
            serviceEvent.ServiceEventType switch
            {
                ServiceEventType.Start => "events.service.started",
                ServiceEventType.Stop => "events.service.stopped",
                _ => throw new ArgumentOutOfRangeException(nameof(serviceEvent))
            };

        public void Publish(ISystemValue payload, bool confirm)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, payload);
            var routingKey = $"{payload.DeviceIdentifier}.{GetKeyForType(payload)}";

            lock (_lockObject)
            {
                var publishChannel = _defaultChannel;

                if (confirm)
                {
                    _ackMap.TryAdd(_confirmChannel.NextPublishSeqNo, payload);
                    publishChannel = _confirmChannel;
                }

                var props = publishChannel.CreateBasicProperties();
                props.ContentType = "application/protobuf";

                publishChannel.BasicPublish(_exchangeName, routingKey, props, stream.ToArray());
            }
        }

        public void Dispose()
        {
            _defaultChannel.Dispose();
            _confirmChannel.Dispose();
        }
    }
}