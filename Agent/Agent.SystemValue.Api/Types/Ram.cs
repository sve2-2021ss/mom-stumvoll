using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class Ram :  IMetric
    {
        [ProtoMember(1)] public uint UsedMb { get; }
        [ProtoMember(2)] public uint TotalMb { get; }
        [ProtoMember(3)] public int MemoryClock { get; }
        [ProtoMember(10)] public string DeviceIdentifier { get; set; }

        public Ram()
        {
        }

        public Ram(uint usedMb, uint totalMb, int memoryClock)
        {
            UsedMb = usedMb;
            TotalMb = totalMb;
            MemoryClock = memoryClock;
        }

        public override string ToString() => $"RamUsage({UsedMb}/{TotalMb})";
    }
}