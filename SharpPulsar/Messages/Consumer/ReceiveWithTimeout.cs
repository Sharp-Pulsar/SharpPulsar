
namespace SharpPulsar.Messages.Consumer
{
    public sealed class ReceiveWithTimeout
    {
        /// <summary>
        /// Every time ConsumerActor receives this message
        /// a message is taken, with timeout, from the IncomingMessageQueue and added into BlockCollection<IMessage<T>> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public int Timeout { get; }
        public ReceiveWithTimeout(int timeout)
        {
            Timeout = timeout;
        }
    }
}
