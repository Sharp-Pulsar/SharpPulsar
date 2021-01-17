using Akka.Actor;
using PulsarAdmin.Models;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces.ISchema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Text;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;

namespace SharpPulsar.Messages.Requests
{
    public sealed class GetBroker
    {
        public IActorRef ReplyTo{ get; }
        public TopicName TopicName { get; }
        public GetBroker(TopicName topicName, IActorRef replyTo)
        {
            TopicName = topicName;
            ReplyTo = replyTo;
        }
    }
    public sealed class GetBrokerRedirect
    {
        public IActorRef ReplyTo{ get; }
        public TopicName TopicName { get; }
        public int RedirectCount { get; }
        public DnsEndPoint BrokerAddress { get; }
        public bool Authoritative { get; }
        public GetBrokerRedirect(TopicName topicName, IActorRef replyTo, int redirectCount, DnsEndPoint brokerAddress, bool authoritative)
        {
            TopicName = topicName;
            ReplyTo = replyTo;
            RedirectCount = redirectCount;
            BrokerAddress = brokerAddress;
            Authoritative = authoritative;
        }
    }
    public sealed class GetPartitionedTopicMetadata
    {
        public TopicName TopicName { get; }
        public IActorRef ReplyTo { get; }
        public GetPartitionedTopicMetadata(TopicName topicName, IActorRef replyTo)
        {
            TopicName = topicName;
            ReplyTo = replyTo;
        }
    }
    public sealed class GetSchema
    {
        public IActorRef ReplyTo { get; }
        public TopicName TopicName { get; }
        public sbyte[] Version { get; }
        public GetSchema(TopicName topicName, sbyte[] version, IActorRef replyTo)
        {
            TopicName = topicName;
            Version = version;
            ReplyTo = replyTo;
        }
    }
    public sealed class GetTopicsUnderNamespace
    {
        public IActorRef ReplyTo { get; }
        public NamespaceName Namespace { get; }
        public Mode Mode { get; }
        public GetTopicsUnderNamespace(NamespaceName @namespace, Mode mode, IActorRef replyTo)
        {
            Namespace = @namespace;
            Mode = mode;
            ReplyTo = replyTo;
        }
    }
    public sealed class GetTopicsUnderNamespaceResponse
    {
        public ImmutableList<string> Topics { get; }
        public GetTopicsUnderNamespaceResponse(IList<string> topics)
        {
            Topics = topics.ToImmutableList();
        }
    }
    public sealed class GetSchemaInfoResponse
    {
        public ISchemaInfo SchemaInfo { get; }
        public GetSchemaInfoResponse(ISchemaInfo schemaInfo)
        {
            SchemaInfo = schemaInfo;
        }
    }
    public sealed class GetBrokerResponse
    {
        public DnsEndPoint LogicalAddress { get; } 
        public DnsEndPoint PhysicalAddress { get; }
        public GetBrokerResponse(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress)
        {
            LogicalAddress = logicalAddress;
            PhysicalAddress = physicalAddress;
        }
    }
}
