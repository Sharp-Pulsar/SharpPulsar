
using SharpPulsar.Common.Entity;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Messages.Producer
{
    public sealed class SentMessage<T>
    {
        public bool Errored { get; } = false;
        public Exception Exception { get; }
        public Message<T> Message { get; }
        public IList<Message<T>> Messages { get; }

        public static SentMessage<T> Create(OpSendMsg<T> op)
        {
            if (op.Msg != null)
                return new SentMessage<T>(op.Msg);
            else
                return new SentMessage<T>(op.Msgs);
        }
        public static SentMessage<T> CreateError(OpSendMsg<T> op, Exception exception)
        {            
            if (op.Msg != null)
                return new SentMessage<T>(op.Msg, exception);
            else
                return new SentMessage<T>(op.Msgs, exception);
        }
        public static SentMessage<T> CreateError(Message<T> msg, Exception exception)
        {
             return new SentMessage<T>(msg, exception);
        }
        public SentMessage(Message<T> message)
        {
            Message = message;
        }
        public SentMessage(IList<Message<T>> messages)
        {
            Messages = messages;
        }
        public SentMessage(Message<T> message, Exception exception)
        {
            Errored = true;
            Exception = exception;
            Message = message;
        }
        public SentMessage(IList<Message<T>> messages, Exception exception)
        {
            Errored = true;
            Exception = exception;
            Messages = messages;
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
    public sealed class TriggerFlush
    {
        public static TriggerFlush Instance = new TriggerFlush();
    }
}
