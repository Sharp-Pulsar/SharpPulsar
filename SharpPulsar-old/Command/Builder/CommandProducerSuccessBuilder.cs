using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.PulsarApi;
using System;

namespace SharpPulsar.Command.Builder
{
    public class CommandProducerSuccessBuilder
    {
        private readonly CommandProducerSuccess _success;
        public CommandProducerSuccessBuilder()
        {
            _success = new CommandProducerSuccess();
        }
        
        public  CommandProducerSuccessBuilder SetRequestId(long requestId)
        {
            _success.RequestId = (ulong)requestId;
            return this;
        }
        public  CommandProducerSuccessBuilder SetProducerName(string producerName)
        {
            _success.ProducerName = producerName;
            return this;
        }
        public  CommandProducerSuccessBuilder SetLastSequenceId(long lastSequenceId)
        {
            _success.LastSequenceId = lastSequenceId;
            return this;

        }
        public  CommandProducerSuccessBuilder SetSchemaVersion(SchemaVersion schemaVersion)
        {
            _success.SchemaVersion = (byte[])(Array)schemaVersion.Bytes();
            return this;
        }
        public  CommandProducerSuccess Build()
        {
            return _success;
        }
    }
}
