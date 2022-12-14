using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using static SharpPulsar.Protocol.Proto.CommandAck;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct ReconsumeLaterWithProperties<T>
    {
        /// <summary>
        /// Fulfils ReconsumeLater<T1>(IMessages<T1> messages, long delayTime, TimeUnit unit)
        /// This message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        public IMessage<T> Message { get; }
        public ImmutableDictionary<string, long> Properties { get; }
        public long DelayTime { get; }
        public AckType AckType { get; }
        public ReconsumeLaterWithProperties(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, long delayTime)
        {
            Message = message;
            Properties = properties.ToImmutableDictionary();
            DelayTime = delayTime;
            AckType = ackType;
        }
    } 
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
        public TimeSpan DelayTime { get; }
        public ImmutableDictionary<string, string> Properties { get; }
        public ReconsumeLaterMessages(IMessages<T> messages, TimeSpan delayTime)
        {
            Messages = messages;
            DelayTime = delayTime;
        }
        public ReconsumeLaterMessages(IMessages<T> message, IDictionary<string, string> customProperties, TimeSpan delayTime)
        {
            Messages = message;
            DelayTime = delayTime;
            Properties = customProperties.ToImmutableDictionary();
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
        public TimeSpan DelayTime { get; }
        public ImmutableDictionary<string, string> Properties { get; }    
        public ReconsumeLaterMessage(IMessage<T> message, TimeSpan delayTime)
        {
            Message = message;
            DelayTime = delayTime;
        }
        public ReconsumeLaterMessage(IMessage<T> message, IDictionary<string, string> customProperties, TimeSpan delayTime)
        {
            Message = message;
            DelayTime = delayTime;
            Properties = customProperties.ToImmutableDictionary();
        }
    }
}
