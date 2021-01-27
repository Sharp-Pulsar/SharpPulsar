namespace SharpPulsar.Messages.Consumer
{
    public sealed class IsConnected
    {
        /// <summary>
        /// When ConsumerActor receives this message
        /// the connection state for that consumer is added into the BlockCollection<bool> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static IsConnected Instance = new IsConnected();
    }
}
