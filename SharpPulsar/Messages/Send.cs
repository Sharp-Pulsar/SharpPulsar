
using System.Collections.Generic;
using System.Collections.Immutable;


namespace SharpPulsar.Messages
{
    public sealed class Send
    {
        public object Message { get; }
        public ImmutableDictionary<string, object> Config { get; }
        /// <summary>
        /// When using PartitionedProducer, messages with same RoutingKey will be sent to the same partition
        /// </summary>
        public string RoutingKey { get; }
        public Send(object message, ImmutableDictionary<string, object> config = null, string routingKey = "default")
        {
            RoutingKey = routingKey;
            Message = message;
            Config = config ?? ImmutableDictionary<string, object>.Empty;
        }
    }
    public sealed class BulkSend
    {
        public List<Send> Messages { get; }
        /// <summary>
        /// All the messages are assumed to be for this topic!
        /// </summary>
        public string Topic { get; }
        /// <summary>
        /// When using PartitionedProducer, messages with same RoutingKey will be sent to the same partition
        /// </summary>
        public string RoutingKey { get; }
        public BulkSend(List<Send> messages, string topic, string routingKey = "default")
        {
            RoutingKey = routingKey;
            Messages = messages;
            Topic = topic;
        }
    }

}
