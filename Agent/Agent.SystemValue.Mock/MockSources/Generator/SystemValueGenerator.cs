using System;
using System.Threading;
using System.Threading.Tasks;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.SystemValue.Mock.MockSources.Generator
{
    public abstract class SystemValueGenerator<T> : ISystemValueGenerator<T>
        where T : ISystemValue, new()
    {
        private readonly int _pollingTimeout;

        protected SystemValueGenerator(int pollingTimeout)
        {
            _pollingTimeout = pollingTimeout;
        }

        public Task Run(CancellationToken token) =>
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    NewSystemValue?.Invoke(GenerateValue());
                    await Task.Delay(_pollingTimeout, token);
                }
            }, token);

        protected abstract T GenerateValue();
        public event Action<T> NewSystemValue;
    }
}