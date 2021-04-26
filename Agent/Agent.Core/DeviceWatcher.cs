using System;
using Agent.Core.PublishApi;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.Core
{
    public sealed class DeviceWatcher : IObserver<ISystemValue>, IDisposable
    {
        private readonly IPublishApi _api;
        private readonly string _identifier;

        public DeviceWatcher(IPublishApi api, string identifier)
        {
            _api = api;
            _identifier = identifier;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
        
        public void OnNext(ISystemValue data)
        {
            _api.Push($"{_identifier}.{data.Type}", data);
        }

        public void Dispose() => _api.Dispose();
    }
}