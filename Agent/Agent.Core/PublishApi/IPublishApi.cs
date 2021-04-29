using System;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.Core.PublishApi
{
    public interface IPublishApi : IDisposable
    {
        void Publish(string deviceIdentifier, ISystemValue payload, bool confirm = false);
    }
}