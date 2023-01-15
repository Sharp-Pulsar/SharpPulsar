using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces.Schema;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;

namespace SharpPulsar.Messages.Requests
{
    public readonly record struct GetBroker
    {
        public TopicName TopicName { get; }
        public GetBroker(TopicName topicName)
        {
            TopicName = topicName;
        }
    }
    public readonly record struct GetSchema
    {
        public TopicName TopicName { get; }
        public byte[] Version { get; }
        public GetSchema(TopicName topicName, byte[] version = null)
        {
            TopicName = topicName;
            Version = version;
        }
    }
    public readonly record struct GetTopicsUnderNamespace
    {
        public NamespaceName Namespace { get; }
        public Mode Mode { get; }
        public string TopicsPattern { get; }
        public string TopicsHash { get; }
        public GetTopicsUnderNamespace(NamespaceName @namespace, Mode mode, string topicsPattern, string topicsHash)
        {
            Namespace = @namespace;
            Mode = mode;
            TopicsPattern = topicsPattern;  
            TopicsHash = topicsHash;    
        }
    }
    public readonly record struct GetTopicsOfNamespaceRetry
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
    public readonly record struct GetTopicsUnderNamespaceResponse
    {
        public ImmutableList<string> Topics { get; }
        public string TopicsHash { get; }
        public bool Changed { get; }
        public bool Filtered { get; }
        public GetTopicsUnderNamespaceResponse(IList<string> topics, string topicsHash, bool changed, bool filtered)
        {
            Topics = topics.ToImmutableList();
            TopicsHash = topicsHash;    
            Changed = changed;  
            Filtered = filtered;    
        }
    }
    public readonly record struct GetSchemaInfoResponse
    {
        public ISchemaInfo SchemaInfo { get; }
        public GetSchemaInfoResponse(ISchemaInfo schemaInfo)
        {
            SchemaInfo = schemaInfo;
        }
    }
    public readonly record struct GetBrokerResponse
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
