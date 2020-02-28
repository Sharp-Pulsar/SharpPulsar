
using System.Collections.Generic;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Send
    {
        public object Message { get; }
        public string Topic { get; }

        public Send(object message, string topic)
        {
            Message = message;
            Topic = topic;
        }
    }
    public sealed class BatchSend
    {
        public List<object> Messages { get; }
        public string Topic { get; }

        public BatchSend(List<object> messages, string topic)
        {
            Messages = messages;
            Topic = topic;
        }
    }
}
