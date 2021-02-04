using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Transaction;
using System;


namespace SharpPulsar.User
{
    public class Producer<T> : IProducer<T>
    {
        public string Topic => throw new NotImplementedException();

        public string ProducerName => throw new NotImplementedException();

        public long LastSequenceId => throw new NotImplementedException();

        public IProducerStats Stats => throw new NotImplementedException();

        public bool Connected => throw new NotImplementedException();

        public long LastDisconnectedTimestamp => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public ITypedMessageBuilder<T> NewMessage()
        {
            throw new NotImplementedException();
        }

        public ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
        {
            throw new NotImplementedException();
        }

        public TypedMessageBuilder<T> NewMessage(ITransaction txn)
        {
            throw new NotImplementedException();
        }

        public IMessageId Send(T message)
        {
            throw new NotImplementedException();
        }
    }
}
