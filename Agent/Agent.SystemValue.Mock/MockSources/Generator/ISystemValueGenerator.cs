using System.Threading;
using System.Threading.Tasks;
using Agent.SystemValue.Api.SystemValueSource;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.SystemValue.Mock.MockSources.Generator
{
    public interface ISystemValueGenerator<out T> : ISystemValueSource<T> where T : ISystemValue
    {
        Task Run(CancellationToken token);
    }
}