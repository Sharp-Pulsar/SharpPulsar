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
        
        public CommandGetLastMessageIdBuilder SetConsumerId(long consumerId)
        {
            _id.ConsumerId = (ulong)consumerId;
            return this;
        }
        public CommandGetLastMessageIdBuilder SetRequestId(long requestId)
        {
            _id.RequestId = (ulong)requestId;
            return this;
        }
        public CommandGetLastMessageId Build()
        {
            return _id;
        }
    }
}
