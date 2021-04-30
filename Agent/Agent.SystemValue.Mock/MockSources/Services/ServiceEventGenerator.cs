using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agent.SystemValue.Api.SystemValueSource;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Services
{
    public class ServiceEventGenerator : ISystemValueSource<ServiceEvent>, ISystemValueGenerator<ServiceEvent>
    {
        private readonly int _sleepLower;
        private readonly int _sleepUpper;
        private readonly Random _random = new();
        private readonly IList<string> _serviceNames;
        private readonly IList<string> _runningServices = new List<string>();

        public ServiceEventGenerator(int sleepLower, int sleepUpper, IList<string> serviceNames)
        {
            _serviceNames = serviceNames;
            _sleepUpper = sleepUpper;
            _sleepLower = sleepLower;
        }

        private void GenerateEvent()
        {
            var d = _random.NextDouble();

            switch (d)
            {
                case < 0.33:
                    if (_runningServices.Count != 0)
                    {
                        var stoppedService = GetRandomElement(_runningServices);
                        _runningServices.Remove(stoppedService);
                        NewSystemValue?.Invoke(new ServiceEvent(stoppedService, ServiceEventType.Start));
                    }

                    break;

                case < 0.66:
                    var startedService = GetRandomElement(_serviceNames);
                    _runningServices.Add(startedService);
                    NewSystemValue?.Invoke(new ServiceEvent(startedService, ServiceEventType.Stop));
                    break;
            }
        }

        private string GetRandomElement(IList<string> list)
        {
            var index = _random.Next(list.Count);
            var elem = list[index];
            return elem;
        }

        public Task Run(CancellationToken token) =>
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    GenerateEvent();
                    await Task.Delay(_random.Next(_sleepLower, _sleepUpper), token);
                }
            }, token);

        public event Action<ServiceEvent> NewSystemValue;
    }
}