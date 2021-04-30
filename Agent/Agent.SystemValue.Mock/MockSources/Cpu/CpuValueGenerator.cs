using System;
using System.Collections.Generic;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Cpu
{
    public class CpuValueGenerator : SystemValueGenerator<Api.Types.Cpu>
    {
        private readonly Random _random = new();
        private readonly int _coreCount;
        private readonly (int lower, int upper) _tempRange;
        private readonly (int lower, int upper) _powerRange;

        public CpuValueGenerator(int pollingTimeout, int coreCount, (int lower, int upper) tempRange,
            (int lower, int upper) powerRange) : base(pollingTimeout)
        {
            _coreCount = coreCount;
            _tempRange = tempRange;
            _powerRange = powerRange;
        }

        protected override Api.Types.Cpu GenerateValue() => new(RandomCpuLoad(), RandomPowerDraw(), RandomCoreTemp());

        private int RandomCpuLoad() => _random.Next(0, 100);

        private IList<int> RandomCoreTemp()
        {
            var list = new List<int>();
            var baseTemp = _random.Next(_tempRange.lower, _tempRange.upper);
            list.Add(baseTemp);
            for (var i = 0; i < _coreCount - 1; i++)
            {
                list.Add(_random.Next(baseTemp - 2, baseTemp + 2));
            }

            return list;
        }

        private int RandomPowerDraw() => _random.Next(_powerRange.lower, _powerRange.upper);
    }
}