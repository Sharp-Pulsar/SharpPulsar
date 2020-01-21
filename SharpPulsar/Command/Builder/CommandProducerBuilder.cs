using SharpPulsar.Common.Protocol;
using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Common.Schema;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Command.Builder
{
    public class CommandProducerBuilder
    {
        private CommandProducer _producer;
        public CommandProducerBuilder()
        {
            _producer = new CommandProducer();
        }
        private CommandProducerBuilder(CommandProducer producer)
        {
            _producer = producer;
        }
        public CommandProducerBuilder SetEncrypted(bool encrypted)
        {
            _producer.Encrypted = encrypted;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetEpoch(long epoch)
        {
            _producer.Epoch = (ulong)epoch;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetProducerId(long producerid)
        {
            _producer.ProducerId = (ulong)producerid;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetProducerName(string producerName)
        {
            if (!string.IsNullOrWhiteSpace(producerName))
                _producer.ProducerName = producerName;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetRequestId(long requestid)
        {
            _producer.RequestId = (ulong)requestid;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetSchema(SchemaInfo schemaInfo)
        {
            if (schemaInfo != null)
                _producer.Schema = GetSchema(schemaInfo);
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetTopic(string topic)
        {
            _producer.Topic = topic;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetMetadata(IDictionary<string, string> metadata)
        {
            _producer.Metadatas.AddRange(CommandUtils.ToKeyValueList(metadata)); 
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducerBuilder SetUserProvidedProducerName(bool userProvidedProducerName)
        {
            _producer.UserProvidedProducerName = userProvidedProducerName;
            return new CommandProducerBuilder(_producer);
        }
        public CommandProducer Build()
        {
            return _producer;
        }
        private Schema GetSchema(SchemaInfo schemaInfo)
        {
            return new SchemaBuilder()
                .SetName(schemaInfo)
                .SetType(schemaInfo)
                .SetSchemaData(schemaInfo).Build();
        }
    }
}
