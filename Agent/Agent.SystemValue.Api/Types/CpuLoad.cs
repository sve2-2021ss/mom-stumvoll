using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class CpuLoad : ISystemValue
    {
        [ProtoMember(1)] public byte LoadPercentage { get; }

        public string Type => "metrics.cpu.load";

        public CpuLoad()
        {
        }

        public CpuLoad(byte loadPercentage)
        {
            LoadPercentage = loadPercentage;
        }
    }
}