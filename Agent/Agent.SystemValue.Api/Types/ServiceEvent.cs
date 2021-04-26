using System;
using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class ServiceEvent : ISystemValue
    {
        [ProtoMember(1)] public string Executable { get; }
        public string Type { get; }

        public ServiceEvent()
        {
        }

        public ServiceEvent(string executable, ServiceEventType serviceEventType)
        {
            Executable = executable;
            Type = serviceEventType switch
            {
                ServiceEventType.Start => "events.service.started",
                ServiceEventType.Stop => "events.service.stopped",
                _ => throw new ArgumentOutOfRangeException(nameof(serviceEventType), serviceEventType, null)
            };
        }
    }

    public enum ServiceEventType
    {
        Start,
        Stop
    }
}