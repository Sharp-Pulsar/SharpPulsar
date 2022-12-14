using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Avro.Util;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using static SharpPulsar.Protocol.Proto.CommandAck;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct AcknowledgeMessage<T>(IMessage<T> Message) : IAcknowledge;
    public readonly record struct AcknowledgeMessageId(IMessageId MessageId) : IAcknowledge;

    public readonly record struct AcknowledgeMessageIds(IList<IMessageId> MessageIds) : IAcknowledge;
    public readonly record struct Subscribe(string TopicName, int NumberOfPartitions);
    public readonly record struct SubscribeAndCreateTopicIfDoesNotExist(string TopicName, bool CreateTopicIfDoesNotExist);

    public readonly record struct SubscribeAndCreateTopicsIfDoesNotExist(List<string> Topics, bool CreateTopicIfDoesNotExist);

    public readonly record struct AcknowledgeWithTxn : IAcknowledge
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
    public readonly record struct AcknowledgeMessages<T>(IMessages<T> Messages) : IAcknowledge;
    public interface IAcknowledge
    {

    }
}
