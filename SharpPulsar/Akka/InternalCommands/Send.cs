
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Send
    {
        public object Message { get; }
        public string Topic { get; }
        public ImmutableDictionary<string, object> Config { get; }
        public Send(object message, string topic, ImmutableDictionary<string, object> config)
        {
            Message = message;
            Topic = topic;
            Config = config;
        }
    }
    public sealed class BatchSend
    {
        public List<Send> Messages { get; }
        public string Topic { get; }
        public bool SeparateTopic { get; }
        public BatchSend(List<Send> messages, string topic, bool separateTopic = false)
        {
            Messages = messages;
            Topic = topic;
            SeparateTopic = separateTopic;
        }
    }
}
