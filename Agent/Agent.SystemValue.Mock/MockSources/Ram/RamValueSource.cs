using System;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Ram
{
    public class RamValueSource : SystemValueGenerator<RamUsage>
    {
        private readonly Random _random = new();
        private readonly uint _maxRamMb;

        public RamValueSource(int pollingTimeout, uint maxRamMb) : base(pollingTimeout)
        {
            _maxRamMb = maxRamMb;
        }

        protected override RamUsage GenerateValue() =>
            new(RandomValue(), _maxRamMb);

        private uint RandomValue() =>
            (uint) _random.Next(0, (int) _maxRamMb);
    }
}