namespace SharpPulsar.Tracker.Messages
{
    public record RemoveTopicMessages
    {
        public RemoveTopicMessages(string topicName)
        {
            TopicName = topicName;   
        }

        public string TopicName { get; }
    }
}
