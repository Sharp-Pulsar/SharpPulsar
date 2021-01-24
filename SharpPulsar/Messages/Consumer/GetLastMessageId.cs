using SharpPulsar.Interfaces;
using System;

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
    public sealed class NullMessageId: IMessageId
    {
        public Exception Exception { get; }
        public NullMessageId(Exception exception)
        {
            Exception = exception;
        }

        public int CompareTo(IMessageId other)
        {
            throw new NotImplementedException();
        }

        public sbyte[] ToByteArray()
        {
            throw new NotImplementedException();
        }
    }
}
