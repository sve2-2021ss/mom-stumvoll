using System;
using System.IO;
using ProtoBuf;
using RabbitMQ.Client;

namespace Agent.Core.PublishApi
{
    public sealed class PublishApi : IPublishApi
    {
        private readonly IModel _model;
        private readonly string _exchangeName;

        public PublishApi(IModel model, string exchangeName)
        {
            _model = model;
            _exchangeName = exchangeName;
        }

        public void Push(string key, object payload)
        {
            Console.WriteLine($"Published to {key}");
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, payload);

            _model.BasicPublish(_exchangeName, key, body: stream.ToArray());
        }

        public void Dispose() => _model.Dispose();
    }
}