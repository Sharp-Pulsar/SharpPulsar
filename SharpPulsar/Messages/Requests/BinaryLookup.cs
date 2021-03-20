using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces.ISchema;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;

namespace SharpPulsar.Messages.Requests
{
    public sealed class GetBroker
    {
        public TopicName TopicName { get; }
        public GetBroker(TopicName topicName)
        {
            TopicName = topicName;
        }
    }
    public sealed class GetBrokerRedirect
    {
        public TopicName TopicName { get; }
        public int RedirectCount { get; }
        public DnsEndPoint BrokerAddress { get; }
        public bool Authoritative { get; }
        public GetBrokerRedirect(TopicName topicName, int redirectCount, DnsEndPoint brokerAddress, bool authoritative)
        {
            TopicName = topicName;
            RedirectCount = redirectCount;
            BrokerAddress = brokerAddress;
            Authoritative = authoritative;
        }
    }
    public sealed class GetSchema
    {
        public TopicName TopicName { get; }
        public sbyte[] Version { get; }
        public GetSchema(TopicName topicName, sbyte[] version = null)
        {
            TopicName = topicName;
            Version = version;
        }
    }
    public sealed class GetTopicsUnderNamespace
    {
        public NamespaceName Namespace { get; }
        public Mode Mode { get; }
        public GetTopicsUnderNamespace(NamespaceName @namespace, Mode mode)
        {
            Namespace = @namespace;
            Mode = mode;
        }
    }
    public sealed class GetTopicsOfNamespaceRetry
    {
        public IActorRef ReplyTo { get; }
        public NamespaceName Namespace { get; }
        public Mode Mode { get; }
        public long RemainingTime { get; }
        public Backoff Backoff { get; }
        public GetTopicsOfNamespaceRetry(NamespaceName @namespace, Backoff backoff, long remainingTime, Mode mode, IActorRef replyTo)
        {
            Namespace = @namespace;
            Mode = mode;
            ReplyTo = replyTo;
            RemainingTime = remainingTime;
            Backoff = backoff;
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
