using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Avro.Util;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct AcknowledgeCumulativeMessage<T>(IMessage<T> Message) : ICumulative;
    public readonly record struct AcknowledgeCumulativeMessageId(IMessageId MessageId) : ICumulative;
    public readonly record struct AcknowledgeCumulativeTxn(IMessageId MessageId, IActorRef Txn) : ICumulative;
    public readonly record struct ReconsumeLaterCumulative<T> : ICumulative
    {
        /// <summary>
        /// Fulfils ReconsumeLaterCumulative<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public TimeSpan DelayTime { get; }
        public ImmutableDictionary<string, string> Properties { get; }  
        public ReconsumeLaterCumulative(IMessage<T> message, TimeSpan delayTime) => (Message, DelayTime, Properties)
            = (message, delayTime, null);
        public ReconsumeLaterCumulative(IMessage<T> message, IDictionary<string, string> properties, TimeSpan delayTime) => (Message, DelayTime, Properties)
            = (message, delayTime, properties.ToImmutableDictionary());
    }
    public interface ICumulative { }
}
