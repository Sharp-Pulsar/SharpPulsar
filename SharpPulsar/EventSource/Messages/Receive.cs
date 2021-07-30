﻿namespace SharpPulsar.EventSource.Messages
{
    public sealed class Receive
    {
        /// <summary>
        /// Every time ConsumerActor receives this message
        /// a message is taken from the IncomingMessageQueue and added into BlockCollection<IMessage<T>> of that consumer
        /// to be consumed at the front end
        /// </summary>
        /// 
        public static Receive Instance = new Receive();
    }
}
