using System.Collections.Generic;
using Agent.SystemValue.Api.Types.Base;
using ProtoBuf;

namespace Agent.SystemValue.Api.Types
{
    [ProtoContract]
    public class Cpu : ISystemValue
    {
        [ProtoMember(1)] public int LoadPercentage { get; }
        [ProtoMember(2)] public int PowerDraw { get; }
        [ProtoMember(3)] public IList<int> CoreTemps { get; }

        public Cpu()
        {
        }

        public Cpu(int loadPercentage, int powerDraw, IList<int> coreTemps)
        {
            LoadPercentage = loadPercentage;
            PowerDraw = powerDraw;
            CoreTemps = coreTemps;
        }

        public override string ToString() => $"CpuLoad({LoadPercentage})";
    }
}