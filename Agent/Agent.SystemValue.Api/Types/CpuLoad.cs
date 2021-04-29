using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class CpuLoad : ISystemValue
    {
        [ProtoMember(1)] public int LoadPercentage { get; }

        public CpuLoad()
        {
        }

        public CpuLoad(int loadPercentage)
        {
            LoadPercentage = loadPercentage;
        }

        public override string ToString() => $"CpuLoad({LoadPercentage})";
    }
}