
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
}
