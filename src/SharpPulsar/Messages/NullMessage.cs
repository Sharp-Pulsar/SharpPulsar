using Akka.Util;
using SharpPulsar.Auth;
using SharpPulsar.Interfaces;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace SharpPulsar.Messages
{
    public readonly record struct NullMessage<T> : IMessage<T>
    {
        public Exception Exception { get; }
        public NullMessage(Exception exception)
        {
            Exception = exception;
        }
        public IDictionary<string, string> Properties => throw new NotImplementedException();

        public ReadOnlySequence<byte> Data => throw new NotImplementedException();

        public T Value => throw new NotImplementedException();

        public IMessageId MessageId => throw new NotImplementedException();

        public long PublishTime => throw new NotImplementedException();

        public long EventTime => throw new NotImplementedException();

        public long SequenceId => throw new NotImplementedException();

        public string ProducerName => throw new NotImplementedException();

        public string Key => throw new NotImplementedException();

        public byte[] KeyBytes => throw new NotImplementedException();

        public byte[] OrderingKey => throw new NotImplementedException();

        public string Topic => throw new NotImplementedException();
        public long Size()
        {
            throw new NotImplementedException();
        }

        public Option<EncryptionContext> EncryptionCtx => throw new NotImplementedException();

        public int RedeliveryCount => throw new NotImplementedException();

        public byte[] SchemaVersion => throw new NotImplementedException();

        public bool Replicated => throw new NotImplementedException();

        public string ReplicatedFrom => throw new NotImplementedException();

        public long? BrokerPublishTime => throw new NotImplementedException();

        public long? Index => throw new NotImplementedException();

        public string GetProperty(string name)
        {
            throw new NotImplementedException();
        }

        public bool HasBase64EncodedKey()
        {
            throw new NotImplementedException();
        }

        public bool HasKey()
        {
            throw new NotImplementedException();
        }

        public bool HasOrderingKey()
        {
            throw new NotImplementedException();
        }

        public bool HasProperty(string name)
        {
            throw new NotImplementedException();
        }

        public void AddProperty(IDictionary<string, string> props)
        {
            throw new NotImplementedException();
        }

        public bool HasBrokerPublishTime()
        {
            throw new NotImplementedException();
        }

        public bool HasIndex()
        {
            throw new NotImplementedException();
        }
    }

    public readonly record struct NullMessages<T> : IMessages<T>
    {
        public Exception Exception { get; }
        public NullMessages(Exception exception)
        {
            Exception = exception;
        }
        public IEnumerator<IMessage<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IList<IMessage<T>> MessageList()
        {
            throw new NotImplementedException();
        }
    }
}
