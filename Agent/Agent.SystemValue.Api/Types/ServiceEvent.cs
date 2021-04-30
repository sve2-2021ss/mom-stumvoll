using System;
using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class ServiceEvent : ISystemValue
    {
        [ProtoMember(1)] public string Executable { get; }

        [ProtoMember(2)] public ServiceEventType ServiceEventType { get; }

        public ServiceEvent()
        {
        }

        public ServiceEvent(string executable, ServiceEventType serviceEventType)
        {
            Executable = executable;
            ServiceEventType = serviceEventType;
        }

        public override string ToString() => $"ServiceEvent({Executable}, {ServiceEventType})";
    }

    [ProtoContract]
    public enum ServiceEventType
    {
        [ProtoEnum] Start = 1,
        [ProtoEnum] Stop = 2
    }
}