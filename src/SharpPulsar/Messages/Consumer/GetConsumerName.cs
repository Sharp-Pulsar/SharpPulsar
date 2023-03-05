namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetConsumerName
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the ConsumerName for that consumer is added into the BlockCollection<string> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static GetConsumerName Instance = new GetConsumerName();
    }
}
