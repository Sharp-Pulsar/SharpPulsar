namespace SharpPulsar.Messages.Requests
{
    public sealed class GetStats
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the stats for that consumer is added into the BlockCollection<IConsumerStats> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static GetStats Instance = new GetStats();
    }
}
