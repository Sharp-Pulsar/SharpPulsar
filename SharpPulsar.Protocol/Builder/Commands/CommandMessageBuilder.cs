using SharpPulsar.Common.PulsarApi;
using System;

namespace SharpPulsar.Command.Builder
{
    public class CommandMessageBuilder
    {
        private CommandMessage _message;
        public CommandMessageBuilder()
        {
            _message = new CommandMessage();
        }
        
        public CommandMessageBuilder SetConsumerId(long consumerid)
        {
            _message.ConsumerId = (ulong)consumerid;
            return this;
        }
        public CommandMessageBuilder SetMessageId(MessageIdData messageId)
        {
            _message.MessageId = messageId;
            return this;
        }
        public CommandMessageBuilder SetRedeliveryCount(int redeliveryCount)
        {
            if (redeliveryCount > 0)
            {
                _message.RedeliveryCount = (uint)redeliveryCount;
            }
            return this;
        }
        public CommandMessage Build()
        {
            if (_message.MessageId is null)
                throw new NullReferenceException("MessageId can not be null");
            if(_message.ConsumerId < 1)
                throw new NullReferenceException("ConsumerId can not be less than one");
            return _message;
        }
    }
}
