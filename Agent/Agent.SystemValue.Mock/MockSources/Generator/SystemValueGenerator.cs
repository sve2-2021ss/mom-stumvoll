using System.Threading;
using System.Threading.Tasks;
using Agent.SystemValue.Api.SystemValueSource;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.SystemValue.Mock.MockSources.Generator
{
    public abstract class SystemValueGenerator<T> : SystemValueSource<T>, ISystemValueGenerator<T>
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
                    Notify(GenerateValue());
                    await Task.Delay(_pollingTimeout, token);
                }
            }, token);

        protected abstract T GenerateValue();
    }
}