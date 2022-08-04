namespace SharpPulsar.Messages
{
    public class RegisteredProducer
    {
        public RegisteredProducer(long producerId, string producerName, string topic, bool isNew = true)
        {
            ProducerId = producerId;
            ProducerName = producerName;
            Topic = topic;
            IsNew = isNew;
        }

        public long ProducerId { get; }
        public string ProducerName { get; }
        public string Topic { get; }
        public bool IsNew { get; }
    }
}
