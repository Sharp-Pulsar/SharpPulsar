using SharpPulsar.Shared;

namespace SharpPulsar.Akka.InternalCommands
{
    public class Payload
    {
        public byte[] Bytes { get; }
        public string CommandType { get; }
        public long RequestId { get; }
        public string Topic { get; }

        public Payload(byte[] bytes, long requestId, string commandType, string topic = "")
        {
            Bytes = bytes;
            RequestId = requestId;
            CommandType = commandType;
            Topic = topic;
        }
        
    }
}
