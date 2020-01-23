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
        
        public CommandUnsubscribeBuilder SetConsumerId(long consumerId)
        {
            _unsubscribe.ConsumerId = (ulong)consumerId;
            return this;
        }
        public CommandUnsubscribeBuilder SetRequestId(long requestId)
        {
            _unsubscribe.RequestId = (ulong)requestId;
            return this;
        }
        public CommandUnsubscribe Build()
        {
            return _unsubscribe;
        }
    }
}
