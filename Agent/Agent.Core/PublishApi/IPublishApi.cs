
using System;

namespace Agent.Core.PublishApi
{
    public interface IPublishApi : IDisposable
    {
        void Publish(string key, object payload);
    }
}