using System;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class BatchReceive
    {
        /// <summary>
        /// Every time ConsumerActor receives this message
        /// a message is taken from the IncomingMessageQueue and added into BlockCollection<IMessages<T>> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static BatchReceive Instance = new BatchReceive();
    }
    public sealed class OnAcknowledge
    {
        public IMessageId MessageId { get; }
        public Exception Exception { get; }
        public OnAcknowledge(IMessageId msgId, Exception exception)
        {
            MessageId = msgId;
            Exception = exception;
        }
    }
    public sealed class OnAcknowledgeCumulative
    {
        public IMessageId MessageId { get; }
        public Exception Exception { get; }
        public OnAcknowledgeCumulative(IMessageId msgId, Exception exception)
        {
            MessageId = msgId;
            Exception = exception;
        }
    }
    public sealed class IncrementNumAcksSent
    {
        public int Sent { get; }
        public IncrementNumAcksSent(int sent)
        {
            Sent = sent;
        }
    }
    public sealed class UnAckedMessageTrackerRemove
    {
        public IMessageId MessageId { get; }
        public UnAckedMessageTrackerRemove(IMessageId messageId)
        {
            MessageId = messageId;
        }
    }
    public sealed class PossibleSendToDeadLetterTopicMessagesRemove
    {
        public IMessageId MessageId { get; }
        public PossibleSendToDeadLetterTopicMessagesRemove(IMessageId messageId)
        {
            MessageId = messageId;
        }
    }
}
