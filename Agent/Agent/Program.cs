using System;
using System.Collections.Generic;
using System.Threading;
using Agent.Core;
using Agent.Core.PublishApi;
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
            using var connection = factory.CreateConnection();
            var cts = new CancellationTokenSource();

            IList<DeviceWatcher> deviceWatchers = new List<DeviceWatcher>();

            for (var i = 0; i < config.GetSection("devices").Get<int>(); i++)
            {
                deviceWatchers.Add(InitializeDevice(
                    i, connection, exchangeName, config.GetSection("dataSourceConfig"), cts.Token));
            }

            Console.ReadKey();
            cts.Cancel();
            foreach (var deviceWatcher in deviceWatchers) deviceWatcher.Dispose();
        }

        private static DeviceWatcher InitializeDevice(
            int index,
            IConnection connection,
            string exchangeName,
            IConfiguration configuration,
            CancellationToken token)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchangeName, "topic", true);

            var api = new PublishApi(channel, exchangeName);
            var deviceWatcher = new DeviceWatcher(api, $"device-{index}");

            var sources = new List<ISystemValueGenerator<ISystemValue>>
            {
                new CpuValueSource(configuration.GetSection("cpu:pollingTimeout").Get<int>()),
                new RamValueSource(
                    configuration.GetSection("ram:pollingTimeout").Get<int>(),
                    configuration.GetSection("ram:maxMb").Get<uint>()
                ),
                new ServiceEventSource(
                    configuration.GetSection("services:sleepLower").Get<int>(),
                    configuration.GetSection("services:sleepUpper").Get<int>(),
                    configuration.GetSection("services:serviceNames").Get<IList<string>>()
                )
            };


            foreach (var mockSource in sources)
            {
                mockSource.Subscribe(deviceWatcher);
                mockSource.Run(token);
            }

            return deviceWatcher;
        }
    }
}