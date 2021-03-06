using System;
using Agent.Core.SystemValueApi;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.Core
{
    public sealed class DeviceWatcher : IDisposable
    {
        private readonly ISystemValueApi _api;
        private readonly string _deviceIdentifier;

        public DeviceWatcher(ISystemValueApi api, string deviceIdentifier)
        {
            _api = api;
            _deviceIdentifier = deviceIdentifier;
        }

        public void OnSystemValue(ISystemValue data)
        {
            data.DeviceIdentifier = _deviceIdentifier;
            Console.WriteLine($"{_deviceIdentifier} -- {data}");
            _api.Publish(data, data is IEvent);
        }

        public void Dispose() => _api.Dispose();
    }
}