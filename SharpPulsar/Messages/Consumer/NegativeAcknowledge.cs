
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class NegativeAcknowledgeMessage<T>
    {
        /// <summary>
        /// Fulfils NegativeAcknowledge<T1>(IMessage<T1> message)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public NegativeAcknowledgeMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
    public sealed class NegativeAcknowledgeMessages<T>
    {
        /// <summary>
        /// Fulfils NegativeAcknowledge<T1>(IMessages<T1> messages)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessages<T> Messages { get; }
        public NegativeAcknowledgeMessages(IMessages<T> messages)
        {
            Messages = messages;
        }
    }
    public sealed class NegativeAcknowledgeMessageId
    {
        /// <summary>
        /// Fulfils NegativeAcknowledge(IMessageId messageId)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessageId MessageId { get; }
        public NegativeAcknowledgeMessageId(IMessageId messageId)
        {
            MessageId = messageId;
        }
    }
}
