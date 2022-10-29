using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class AcknowledgeCumulativeMessage<T> : ICumulative
    {
        /// <summary>
        /// Fulfils AcknowledgeCumulative<T1>(IMessage<T1> message)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public AcknowledgeCumulativeMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
    public sealed class AcknowledgeCumulativeMessageId : ICumulative
    {
        /// <summary>
        /// Fulfils AcknowledgeCumulative(IMessageId messageId)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessageId MessageId { get; }
        public AcknowledgeCumulativeMessageId(IMessageId messageid)
        {
            MessageId = messageid;
        }
    }
    public sealed class AcknowledgeCumulativeTxn : ICumulative
    {
        /// <summary>
        /// Fulfils AcknowledgeCumulative(IMessageId messageId, IActorRef txn)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessageId MessageId { get; }
        public IActorRef Txn { get; }
        public AcknowledgeCumulativeTxn(IMessageId messageid, IActorRef txn)
        {
            MessageId = messageid;
            Txn = txn;
        }
    }
    public sealed class ReconsumeLaterCumulative<T> : ICumulative
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
        public IDictionary<string, string> Properties { get; }  
        public ReconsumeLaterCumulative(IMessage<T> message, TimeSpan delayTime)
        {
            Message = message;
            DelayTime = delayTime;
        }
        public ReconsumeLaterCumulative(IMessage<T> message, IDictionary<string, string> properties, TimeSpan delayTime)
        {
            Message = message;
            DelayTime = delayTime;
            Properties = properties;    
        }
    }
    public interface ICumulative { }
}
