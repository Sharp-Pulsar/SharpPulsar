using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandCloseConsumerBuilder
    {
        private readonly CommandCloseConsumer _consumer;
        public CommandCloseConsumerBuilder()
        {
            _consumer = new CommandCloseConsumer();
        }
        public CommandCloseConsumerBuilder(CommandCloseConsumer consumer)
        {
            _consumer = consumer;
        }
        public CommandCloseConsumerBuilder SetConsumerId(long consumerid)
        {
            _consumer.ConsumerId = (ulong)consumerid;
            return new CommandCloseConsumerBuilder(_consumer);
        }
        public CommandCloseConsumerBuilder SetRequestId(long requestid)
        {
            _consumer.RequestId = (ulong)requestid;
            return new CommandCloseConsumerBuilder(_consumer);
        }
        public CommandCloseConsumer Build()
        {
            return _consumer;
        }
    }
}
