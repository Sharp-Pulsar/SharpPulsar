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
    public record struct OnAcknowledge(IMessageId MessageId, Exception Exception);
   
    public record struct OnAcknowledgeCumulative(IMessageId MessageId, Exception Exception);
   
    public record struct IncrementNumAcksSent(int Sent);
    public record struct UnAckedMessageTrackerRemove(IMessageId MessageId);
   
    public record struct PossibleSendToDeadLetterTopicMessagesRemove(IMessageId MessageId);
}
