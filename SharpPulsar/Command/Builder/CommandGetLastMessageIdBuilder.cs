using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandGetLastMessageIdBuilder
    {
        private readonly CommandGetLastMessageId _id;
        public CommandGetLastMessageIdBuilder()
        {
            _id = new CommandGetLastMessageId();
        }
        private CommandGetLastMessageIdBuilder(CommandGetLastMessageId id)
        {
            _id = id;
        }
        public CommandGetLastMessageIdBuilder SetConsumerId(long consumerId)
        {
            _id.ConsumerId = (ulong)consumerId;
            return new CommandGetLastMessageIdBuilder(_id);
        }
        public CommandGetLastMessageIdBuilder SetRequestId(long requestId)
        {
            _id.RequestId = (ulong)requestId;
            return new CommandGetLastMessageIdBuilder(_id);
        }
        public CommandGetLastMessageId Build()
        {
            return _id;
        }
    }
}
