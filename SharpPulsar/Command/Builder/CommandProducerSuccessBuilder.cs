using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.PulsarApi;
using System;

namespace SharpPulsar.Command.Builder
{
    public class CommandProducerSuccessBuilder
    {
        private static CommandProducerSuccess _success;
        public CommandProducerSuccessBuilder()
        {
            _success = new CommandProducerSuccess();
        }
        private CommandProducerSuccessBuilder(CommandProducerSuccess success)
        {
            _success = success;
        }
        public static CommandProducerSuccessBuilder SetRequestId(long requestId)
        {
            _success.RequestId = (ulong)requestId;
            return new CommandProducerSuccessBuilder(_success);
        }
        public static CommandProducerSuccessBuilder SetProducerName(string producerName)
        {
            _success.ProducerName = producerName;
            return new CommandProducerSuccessBuilder(_success);
        }
        public static CommandProducerSuccessBuilder SetLastSequenceId(long lastSequenceId)
        {
            _success.LastSequenceId = lastSequenceId;
            return new CommandProducerSuccessBuilder(_success);

        }
        public static CommandProducerSuccessBuilder SetSchemaVersion(SchemaVersion schemaVersion)
        {
            _success.SchemaVersion = (byte[])(Array)schemaVersion.Bytes();
            return new CommandProducerSuccessBuilder(_success);
        }
        public static CommandProducerSuccess Build()
        {
            return _success;
        }
    }
}
