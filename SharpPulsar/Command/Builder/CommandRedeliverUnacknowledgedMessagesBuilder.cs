using SharpPulsar.Common.PulsarApi;
using System.Collections.Generic;

namespace SharpPulsar.Command.Builder
{
    public class CommandRedeliverUnacknowledgedMessagesBuilder
    {
        private CommandRedeliverUnacknowledgedMessages _messages;
        public CommandRedeliverUnacknowledgedMessagesBuilder()
        {
            _messages = new CommandRedeliverUnacknowledgedMessages();
        }
        
        public CommandRedeliverUnacknowledgedMessagesBuilder SetConsumerId(long consumerId)
        {
            _messages.ConsumerId = (ulong)consumerId;
            return this;
        }
        public CommandRedeliverUnacknowledgedMessagesBuilder SetMessageIds(IList<MessageIdData> messageIds)
        {
            _messages.MessageIds.AddRange(messageIds);
            return this;
        }
        public CommandRedeliverUnacknowledgedMessages Build()
        {
            return _messages;
        }
    }
}
