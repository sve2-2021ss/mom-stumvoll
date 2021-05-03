using System;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.Core.SystemValueApi
{
    public interface ISystemValueApi : IDisposable
    {
        void Publish(ISystemValue payload, bool confirm = false);
    }
}