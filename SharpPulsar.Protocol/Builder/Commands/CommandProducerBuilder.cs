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
        
        public CommandProducerBuilder SetEncrypted(bool encrypted)
        {
            _producer.Encrypted = encrypted;
            return this;
        }
        public CommandProducerBuilder SetEpoch(long epoch)
        {
            _producer.Epoch = (ulong)epoch;
            return this;
        }
        public CommandProducerBuilder SetProducerId(long producerid)
        {
            _producer.ProducerId = (ulong)producerid;
            return this;
        }
        public CommandProducerBuilder SetProducerName(string producerName)
        {
            if (!string.IsNullOrWhiteSpace(producerName))
                _producer.ProducerName = producerName;
            return this;
        }
        public CommandProducerBuilder SetRequestId(long requestid)
        {
            _producer.RequestId = (ulong)requestid;
            return this;
        }
        public CommandProducerBuilder SetSchema(SchemaInfo schemaInfo)
        {
            if (schemaInfo != null)
                _producer.Schema = GetSchema(schemaInfo);
            return this;
        }
        public CommandProducerBuilder SetTopic(string topic)
        {
            _producer.Topic = topic;
            return this;
        }
        public CommandProducerBuilder SetMetadata(IDictionary<string, string> metadata)
        {
            _producer.Metadatas.AddRange(CommandUtils.ToKeyValueList(metadata)); 
            return this;
        }
        public CommandProducerBuilder SetUserProvidedProducerName(bool userProvidedProducerName)
        {
            _producer.UserProvidedProducerName = userProvidedProducerName;
            return this;
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
