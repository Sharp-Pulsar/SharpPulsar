namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct RemoveTopicMessages
    {
        public RemoveTopicMessages(string topicName)
        {
            TopicName = topicName;   
        }

        public string TopicName { get; }
    }
}
