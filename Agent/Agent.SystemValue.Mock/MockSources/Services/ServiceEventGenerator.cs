using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Mock.MockSources.Generator;

namespace Agent.SystemValue.Mock.MockSources.Services
{
    public class ServiceEventGenerator : ISystemValueGenerator<ServiceEvent>
    {
        private readonly int _sleepLower;
        private readonly int _sleepUpper;
        private readonly Random _random = new();
        private readonly IList<string> _stoppedServices;
        private readonly IList<string> _runningServices = new List<string>();

        public ServiceEventGenerator(int sleepLower, int sleepUpper, IList<string> serviceNames)
        {
            _stoppedServices = serviceNames;
            _sleepUpper = sleepUpper;
            _sleepLower = sleepLower;
        }

        private void GenerateEvent()
        {
            var d = _random.NextDouble();

            switch (d)
            {
                case < 0.33:
                    if (_runningServices.Any())
                    {
                        var stoppedService = GetRandomElement(_runningServices);
                        _runningServices.Remove(stoppedService);
                        _stoppedServices.Add(stoppedService);
                        NewSystemValue?.Invoke(new ServiceEvent(stoppedService, ServiceEventType.Stop));
                    }

                    break;
                case < 0.66:
                    if (_stoppedServices.Any())
                    {
                        var startedService = GetRandomElement(_stoppedServices);
                        _runningServices.Add(startedService);
                        _stoppedServices.Remove(startedService);
                        NewSystemValue?.Invoke(new ServiceEvent(startedService, ServiceEventType.Start));
                    }

                    break;
            }
        }

        private string GetRandomElement(IList<string> list) => list[_random.Next(list.Count)];

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