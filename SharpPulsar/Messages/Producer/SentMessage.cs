
using SharpPulsar.Common.Entity;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar.Messages.Producer
{
    public sealed class SentMessage<T>
    {
        public bool Errored { get; } = false;
        public Exception Exception { get; }
        public IMessageId MessageId { get; }
        public IMessage<T> Message { get; }
        public SentMessage(IMessage<T> message, IMessageId messageId)
        {
            Message = message;
            MessageId = messageId;
        }
        public SentMessage(Exception exception)
        {
            Errored = true;
            Exception = exception;
        }
    }

    public sealed class ProducerCreation
    {
        public bool Errored { get; } = false;
        public Exception Exception { get; }
        public ProducerResponse Producer { get; }
        public ProducerCreation(ProducerResponse producer)
        {
            Producer = producer;
        }
        public ProducerCreation(Exception exception)
        {
            Errored = true;
            Exception = exception;
        }
    }
}
