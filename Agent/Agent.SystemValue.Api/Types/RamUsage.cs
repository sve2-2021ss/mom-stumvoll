using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class RamUsage : ISystemValue
    {
        [ProtoMember(1)] public uint UsedMb { get; }
        [ProtoMember(2)] public uint TotalMb { get; }

        public string Type => "metrics.ram.usage";

        public RamUsage()
        {
        }

        public RamUsage(uint usedMb, uint totalMb)
        {
            UsedMb = usedMb;
            TotalMb = totalMb;
        }

        public override string ToString() => $"RamUsage({UsedMb}/{TotalMb})";
    }
}