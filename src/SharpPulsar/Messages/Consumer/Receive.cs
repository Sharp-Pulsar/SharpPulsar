
using System;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class Receive
    {
        /// <summary>
        /// Every time ConsumerActor receives this message
        /// a message is taken from the IncomingMessageQueue and added into BlockCollection<IMessage<T>> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public readonly TimeSpan Time = TimeSpan.Zero;
        public static Receive Instance = new Receive();

        public Receive()
        {
        }
        public Receive(TimeSpan time)
        {
            Time = time;
        }

    } 
}
