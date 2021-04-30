using System;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Ram
{
    public class RamValueGenerator : SystemValueGenerator<Api.Types.Ram>
    {
        private readonly Random _random = new();
        private readonly uint _maxRamMb;
        private readonly (int lower, int upper) _clockRange;


        public RamValueGenerator(int pollingTimeout, uint maxRamMb, (int lower, int upper) clockRange) : base(
            pollingTimeout)
        {
            _maxRamMb = maxRamMb;
            _clockRange = clockRange;
        }

        protected override Api.Types.Ram GenerateValue() =>
            new(RandomValue(), _maxRamMb, RandomClock());

        private uint RandomValue() =>
            (uint) _random.Next(0, (int) _maxRamMb);

        private int RandomClock() => _random.Next(_clockRange.lower, _clockRange.upper);
    }
}