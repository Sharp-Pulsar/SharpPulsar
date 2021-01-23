using BAMCIS.Util.Concurrent;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class ReconsumeLaterMessages<T>
    {
        /// <summary>
        /// Fulfils ReconsumeLater<T1>(IMessages<T1> messages, long delayTime, TimeUnit unit)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessages<T> Messages { get; }
        public long DelayTime { get; }
        public TimeUnit TimeUnit { get; }
        public ReconsumeLaterMessages(IMessages<T> messages, long delayTime, TimeUnit unit)
        {
            Messages = messages;
            DelayTime = delayTime;
            TimeUnit = unit;
        }
    } 
    public sealed class ReconsumeLaterMessage<T>
    {
        /// <summary>
        /// Fulfils ReconsumeLater<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public long DelayTime { get; }
        public TimeUnit TimeUnit { get; }
        public ReconsumeLaterMessage(IMessage<T> message, long delayTime, TimeUnit unit)
        {
            Message = message;
            DelayTime = delayTime;
            TimeUnit = unit;
        }
    }
}
