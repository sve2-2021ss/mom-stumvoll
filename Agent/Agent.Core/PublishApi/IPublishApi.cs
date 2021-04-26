
using System;

namespace Agent.Core.PublishApi
{
    public interface IPublishApi : IDisposable
    {
        void Push(string key, object payload);
    }
}