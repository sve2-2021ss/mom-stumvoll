using System;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.SystemValue.Api.SystemValueSource
{
    public interface ISystemValueSource<out T> where T : ISystemValue
    {
        event Action<T> NewSystemValue;
    }
}