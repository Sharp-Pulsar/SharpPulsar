using Akka.Actor;
using System.Collections.Generic;

namespace SharpPulsar.Messages.Consumer
{
    public record AcknowledgeMessage<T>(IMessage<T> Message) : IAcknowledge;
    public record AcknowledgeMessageId(IMessageId MessageId) : IAcknowledge;

    public record AcknowledgeMessageIds(IList<IMessageId> MessageIds) : IAcknowledge;
    public record Subscribe(string TopicName, int NumberOfPartitions);
    public record SubscribeAndCreateTopicIfDoesNotExist(string TopicName, bool CreateTopicIfDoesNotExist);

    public record SubscribeAndCreateTopicsIfDoesNotExist(List<string> Topics, bool CreateTopicIfDoesNotExist);

    public record AcknowledgeWithTxn : IAcknowledge
    {
        /// <summary>
        /// Fulfils DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessageId MessageId { get; }
        public AckType AckType { get; }
        public IDictionary<string, long> Properties { get; }
        public IActorRef Txn { get; }
        public AcknowledgeWithTxn(IMessageId messageId, IActorRef txn) => (MessageId, AckType, Properties, Txn) = (messageId, AckType.Individual, new Dictionary<string, long>(), txn);
        
        public AcknowledgeWithTxn(IMessageId messageId, IDictionary<string, long> properties, IActorRef txn) => (MessageId, AckType, Properties, Txn) = (messageId, AckType.Individual, properties, txn);
        
    }
    
    public sealed class AcknowledgeWithTxnMessages : IAcknowledge
    {
        /// <summary>
        /// Fulfils DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IList<IMessageId> MessageIds { get; }
        public AckType AckType { get; }
        public IDictionary<string, long> Properties { get; }
        public IActorRef Txn { get; }
        public AcknowledgeWithTxnMessages(IList<IMessageId> messageIds,IDictionary<string, long> properties, IActorRef txn)
        {
            MessageIds = messageIds;
            AckType = AckType.Cumulative;
            Properties = properties;
            Txn = txn;
        }
        public AcknowledgeWithTxnMessages(IList<IMessageId> messageIds, IActorRef txn)
        {
            MessageIds = messageIds;
            AckType = AckType.Cumulative;
            Properties = new Dictionary<string, long>();
            Txn = txn;
        }
    }
    public record AcknowledgeMessages<T>(IMessages<T> Messages) : IAcknowledge;
    public interface IAcknowledge
    {

    }
}
