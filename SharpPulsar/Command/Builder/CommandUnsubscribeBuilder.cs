using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandUnsubscribeBuilder
    {
        private readonly CommandUnsubscribe _unsubscribe;
        public CommandUnsubscribeBuilder()
        {
            _unsubscribe = new CommandUnsubscribe();
        }
        private CommandUnsubscribeBuilder(CommandUnsubscribe unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }
        public CommandUnsubscribeBuilder SetConsumerId(long consumerId)
        {
            _unsubscribe.ConsumerId = (ulong)consumerId;
            return new CommandUnsubscribeBuilder(_unsubscribe);
        }
        public CommandUnsubscribeBuilder SetRequestId(long requestId)
        {
            _unsubscribe.RequestId = (ulong)requestId;
            return new CommandUnsubscribeBuilder(_unsubscribe);
        }
        public CommandUnsubscribe Build()
        {
            return _unsubscribe;
        }
    }
}
