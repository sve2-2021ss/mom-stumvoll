using System;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Cpu
{
    public class CpuValueSource : SystemValueGenerator<CpuLoad>
    {
        private readonly Random _random = new();

        public CpuValueSource(int pollingTimeout) : base(pollingTimeout)
        {
        }

        protected override CpuLoad GenerateValue() => new(RandomValue());

        private byte RandomValue() => (byte) _random.Next(0, 100);
    }
}