namespace SharpPulsar.Messages.Requests
{
    public readonly record struct GetTopic
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the topic for that consumer is added into the BlockCollection<string> of that consumer
        /// to be consumed at the front end
        /// </summary>
        public static GetTopic Instance = new GetTopic();
    }
}
namespace SharpPulsar.Messages.Requests
{
    public readonly record struct GetTopicNameWithoutPartition
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the topic for that consumer is added into the BlockCollection<string> of that consumer
        /// to be consumed at the front end
        /// </summary>
        public static GetTopicNameWithoutPartition Instance = new GetTopicNameWithoutPartition();
    }
}