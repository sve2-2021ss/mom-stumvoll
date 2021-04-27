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

        [ProtoMember(2)] public ServiceEventType ServiceEventType { get; set; }

        public ServiceEvent()
        {
        }

        public ServiceEvent(string executable, ServiceEventType serviceEventType)
        {
            Executable = executable;
            ServiceEventType = serviceEventType;
            Type = serviceEventType switch
            {
                ServiceEventType.Start => "events.service.started",
                ServiceEventType.Stop => "events.service.stopped",
                _ => throw new ArgumentOutOfRangeException(nameof(serviceEventType), serviceEventType, null)
            };
        }
    }

    [ProtoContract]
    public enum ServiceEventType
    {
        [ProtoEnum] Start = 0,
        [ProtoEnum] Stop = 1
    }
}