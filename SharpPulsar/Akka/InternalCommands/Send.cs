
using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Send
    {
        public object Message { get; }
        public string Topic { get; }
        public ImmutableDictionary<string, object> Config { get; }
        /// <summary>
        /// When using PartitionedProducer, messages with same RoutingKey will be sent to the same partition
        /// </summary>
        public string RoutingKey { get; }
        public Send(object message, string topic, ImmutableDictionary<string, object> config, string routingKey = "default")
        {
            RoutingKey = routingKey;
            Message = message;
            Topic = topic;
            Config = config;
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
