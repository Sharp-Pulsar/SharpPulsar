
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetSubscription
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the subscription for that consumer is added into the BlockCollection<string> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static GetSubscription Instance = new GetSubscription();
    }
}
