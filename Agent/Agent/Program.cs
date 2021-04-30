using System;
using System.Collections.Generic;
using System.Threading;
using Agent.Core;
using Agent.Core.SystemValueApi;
using Agent.SystemValue.Api.Types.Base;
using Agent.SystemValue.Mock.MockSources.Cpu;
using Agent.SystemValue.Mock.MockSources.Generator;
using Agent.SystemValue.Mock.MockSources.Ram;
using Agent.SystemValue.Mock.MockSources.Services;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Agent.App
{
    internal static class Program
    {
        private static void Main()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var host = config["brokerHost"];
            var exchangeName = config["exchangeName"];

            if (host == null)
            {
                Console.Error.WriteLine("Host not set");
                return;
            }

            if (exchangeName == null)
            {
                Console.Error.WriteLine("Exchange name not set");
                return;
            }

            var factory = new ConnectionFactory {HostName = host};
            var cts = new CancellationTokenSource();

            IList<(DeviceWatcher, IConnection)> deviceWatchers = new List<(DeviceWatcher, IConnection)>();

            for (var i = 0; i < config.GetSection("devices").Get<int>(); i++)
            {
                var connection = factory.CreateConnection();
                var deviceWatcher = InitializeDevice(
                    i, connection, exchangeName, config.GetSection("dataSourceConfig"), cts.Token);
                deviceWatchers.Add((deviceWatcher, connection));
            }

            Console.ReadKey();
            cts.Cancel();
            foreach (var (deviceWatcher, connection) in deviceWatchers)
            {
                deviceWatcher.Dispose();
                connection.Dispose();
            }
        }

        private static DeviceWatcher InitializeDevice(
            int index,
            IConnection connection,
            string exchangeName,
            IConfiguration configuration,
            CancellationToken token)
        {
            var api = new SystemValueApi(exchangeName, connection);
            var deviceWatcher = new DeviceWatcher(api, $"device-{index}");

            var sources = new List<ISystemValueGenerator<ISystemValue>>
            {
                new CpuValueGenerator(
                    configuration.GetSection("cpu:pollingTimeout").Get<int>(),
                    configuration.GetSection("cpu:coreCount").Get<int>(),
                    (configuration.GetSection("cpu:temps:lower").Get<int>(),
                        configuration.GetSection("cpu:temps:upper").Get<int>()),
                    (configuration.GetSection("cpu:powerDraw:lower").Get<int>(),
                        configuration.GetSection("cpu:powerDraw:upper").Get<int>())
                ),
                new RamValueGenerator(
                    configuration.GetSection("ram:pollingTimeout").Get<int>(),
                    configuration.GetSection("ram:maxMb").Get<uint>(),
                    (configuration.GetSection("ram:clock:lower").Get<int>(),
                        configuration.GetSection("ram:clock:upper").Get<int>())
                ),
                new ServiceEventGenerator(
                    configuration.GetSection("services:sleepLower").Get<int>(),
                    configuration.GetSection("services:sleepUpper").Get<int>(),
                    configuration.GetSection("services:serviceNames").Get<IList<string>>()
                )
            };


            foreach (var mockSource in sources)
            {
                mockSource.NewSystemValue += deviceWatcher.OnSystemValue;
                mockSource.Run(token);
            }

            return deviceWatcher;
        }
    }
}