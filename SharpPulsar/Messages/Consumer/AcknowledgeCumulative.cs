using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class AcknowledgeCumulativeMessage<T>
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
    public sealed class AcknowledgeCumulativeMessageId
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
    public sealed class AcknowledgeCumulativeTxn
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
    public sealed class ReconsumeLaterCumulative<T>
    {
        /// <summary>
        /// Fulfils ReconsumeLaterCumulative<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public long DelayTime { get; }
        public ReconsumeLaterCumulative(IMessage<T> message, long delayTime)
        {
            Message = message;
            DelayTime = delayTime;
        }
    }
}
