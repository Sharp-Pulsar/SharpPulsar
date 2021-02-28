using Akka.Actor;
using SharpPulsar.Interfaces;
using System.Collections.Generic;
using static SharpPulsar.Protocol.Proto.CommandAck;

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
    public sealed class Subscribe
    {
        public string TopicName { get; }
        public int NumberOfPartitions { get; }
        public Subscribe(string topicName, int numberPartitions)
        {
            TopicName = topicName;
            NumberOfPartitions = numberPartitions;
        }
    }
    public sealed class SubscribeAndCreateTopicIfDoesNotExist
    {
        public string TopicName { get; }
        public bool CreateTopicIfDoesNotExist { get; }
        public SubscribeAndCreateTopicIfDoesNotExist(string topicName, bool createTopicIfDoesNotExist)
        {
            TopicName = topicName;
            CreateTopicIfDoesNotExist = createTopicIfDoesNotExist;
        }
    }
    public sealed class AcknowledgeWithTxn
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
        public AcknowledgeWithTxn(IMessageId messageId, IActorRef txn)
        {
            MessageId = messageId;
            AckType = AckType.Individual;
            Properties = new Dictionary<string, long>();
            Txn = txn;
        }
        public AcknowledgeWithTxn(IMessageId messageId, IDictionary<string, long> properties, IActorRef txn)
        {
            MessageId = messageId;
            AckType = AckType.Individual;
            Properties = properties;
            Txn = txn;
        }
    }
    
    public sealed class AcknowledgeWithTxnMessages
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
    public sealed class AcknowledgeMessages<T>
    {
        /// <summary>
        /// Fulfils Acknowledge<T1>(IMessages<T1> messages)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessages<T> Messages { get; }
        public AcknowledgeMessages(IMessages<T> messages)
        {
            Messages = messages;
        }
    }
}
