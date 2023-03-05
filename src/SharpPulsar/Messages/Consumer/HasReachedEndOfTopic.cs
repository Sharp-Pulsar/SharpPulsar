
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct HasReachedEndOfTopic
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the response is added into the BlockCollection<bool> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static HasReachedEndOfTopic Instance = new HasReachedEndOfTopic();
    } 
}
