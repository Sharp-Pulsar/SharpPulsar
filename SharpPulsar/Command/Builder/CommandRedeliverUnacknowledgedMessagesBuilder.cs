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
        private CommandRedeliverUnacknowledgedMessagesBuilder(CommandRedeliverUnacknowledgedMessages messages)
        {
            _messages = messages;
        }
        public CommandRedeliverUnacknowledgedMessagesBuilder SetConsumerId(long consumerId)
        {
            _messages.ConsumerId = (ulong)consumerId;
            return new CommandRedeliverUnacknowledgedMessagesBuilder(_messages);
        }
        public CommandRedeliverUnacknowledgedMessagesBuilder SetMessageIds(IList<MessageIdData> messageIds)
        {
            _messages.MessageIds.AddRange(messageIds);//Was generated as readonly, I editted
            return new CommandRedeliverUnacknowledgedMessagesBuilder(_messages);
        }
        public CommandRedeliverUnacknowledgedMessages Build()
        {
            return _messages;
        }
    }
}
