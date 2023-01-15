
using System;
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct ReceiveWithTimeout
    {
        /// <summary>
        /// Every time ConsumerActor receives this message
        /// a message is taken, with timeout, from the IncomingMessageQueue and added into BlockCollection<IMessage<T>> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public long Timeout { get; }
        public ReceiveWithTimeout(TimeSpan timeout)
        {
            Timeout = (long)timeout.TotalSeconds;
        }
    }
}
