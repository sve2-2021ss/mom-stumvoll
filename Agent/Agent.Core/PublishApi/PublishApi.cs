using System;
using System.Collections.Concurrent;
using System.IO;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;
using RabbitMQ.Client;

namespace Agent.Core.PublishApi
{
    public sealed class PublishApi : IPublishApi
    {
        private readonly string _exchangeName;
        private readonly IModel _defaultChannel;
        private readonly IModel _confirmChannel;
        private readonly object _lockObject = new();
        private readonly ConcurrentQueue<(ulong tag, string deviceIdentifier, ISystemValue value)> _retryQueue = new();

        public PublishApi(string exchangeName, IConnection connection)
        {
            _exchangeName = exchangeName;
            _defaultChannel = connection.CreateModel();
            _defaultChannel.ExchangeDeclare(exchangeName, "topic", true);

            _confirmChannel = connection.CreateModel();
            _confirmChannel.ExchangeDeclare(exchangeName, "topic", true);
            _confirmChannel.ConfirmSelect();
            _confirmChannel.BasicAcks += (_, args) => { HandleServerAck(args.DeliveryTag, args.Multiple, false); };
            _confirmChannel.BasicNacks += (_, args) => { HandleServerAck(args.DeliveryTag, args.Multiple, true); };
        }

        private void HandleServerAck(ulong tag, bool multiple, bool shouldResend)
        {
            bool TagComparer(ulong queueTag) =>
                multiple ? queueTag >= tag : queueTag == tag;

            while (_retryQueue.TryPeek(out var result) && TagComparer(result.tag))
            {
                if (_retryQueue.TryDequeue(out var resendValue) && shouldResend)
                {
                    Publish(resendValue.deviceIdentifier, resendValue.value, true);
                }
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

        public void Publish(string deviceIdentifier, ISystemValue payload, bool confirm)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, payload);
            var routingKey = $"{deviceIdentifier}.{GetKeyForType(payload)}";

            lock (_lockObject)
            {
                if (confirm)
                {
                    _retryQueue.Enqueue((_confirmChannel.NextPublishSeqNo, deviceIdentifier, payload));
                    _confirmChannel.BasicPublish(_exchangeName, routingKey, body: stream.ToArray());
                }
                else
                {
                    _defaultChannel.BasicPublish(_exchangeName, routingKey, body: stream.ToArray());
                }
            }
        }

        public void Dispose()
        {
            _defaultChannel.Dispose();
            _confirmChannel.Dispose();
        }
    }
}