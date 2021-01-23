namespace SharpPulsar.Messages.Consumer
{
    public sealed class GetLastMessageId
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the last messageid for that consumer is added into the BlockCollection<IMessageId> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static GetLastMessageId Instance = new GetLastMessageId();
    }
}
