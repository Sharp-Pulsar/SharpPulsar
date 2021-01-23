using SharpPulsar.Interfaces;
using System.Collections.Generic;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class AcknowledgeMessage<T>
    {
        /// <summary>
        /// Fulfils Acknowledge<T1>(IMessage<T1> message)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public AcknowledgeMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
    public sealed class AcknowledgeMessageId
    {
        /// <summary>
        /// Fulfils Acknowledge(IMessageId messageId)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessageId MessageId { get; }
        public AcknowledgeMessageId(IMessageId messageId)
        {
            MessageId = messageId;
        }
    }
    public sealed class AcknowledgeMessageIds
    {
        /// <summary>
        /// Fulfils Acknowledge(IList<IMessageId> messageIdList)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IList<IMessageId> MessageIds { get; }
        public AcknowledgeMessageIds(IList<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }
    }
    public sealed class AcknowledgeMessages<T>
    {
        /// <summary>
        /// Fulfils Acknowledge<T1>(IMessages<T1> messages)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessages<T> Message { get; }
        public AcknowledgeMessages(IMessages<T> message)
        {
            Message = message;
        }
    }
}
