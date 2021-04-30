using System;
using Agent.Core.PublishApi;
using Agent.SystemValue.Api.Types;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.Core
{
    public sealed class DeviceWatcher : IDisposable
    {
        private readonly IPublishApi _api;
        private readonly string _deviceIdentifier;

        public DeviceWatcher(IPublishApi api, string deviceIdentifier)
        {
            _api = api;
            _deviceIdentifier = deviceIdentifier;
        }

        public void OnSystemValue(ISystemValue data)
        {
            Console.WriteLine($"{_deviceIdentifier} -- {data}");
            _api.Publish(_deviceIdentifier, data, data is ServiceEvent);
        }

        public void Dispose() => _api.Dispose();
    }
}