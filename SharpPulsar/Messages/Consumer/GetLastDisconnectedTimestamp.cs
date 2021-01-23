namespace SharpPulsar.Messages.Consumer
{
    public sealed class GetLastDisconnectedTimestamp
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the LastDisconnectedTimestamp for that consumer is added into the BlockCollection<long> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static GetLastDisconnectedTimestamp Instance = new GetLastDisconnectedTimestamp();
    }
}
