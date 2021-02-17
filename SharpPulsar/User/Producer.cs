using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Queues;
using System;


namespace SharpPulsar.User
{
    public class Producer<T> : IProducer<T>
    {
        private readonly IActorRef _producerActor;
        private readonly ProducerQueueCollection<T> _queue;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;

        public Producer(IActorRef producer, ProducerQueueCollection<T> queue, ISchema<T> schema, ProducerConfigurationData conf)
        {
            _producerActor = producer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => _producerActor.AskFor<string>(GetTopic.Instance);

        public string ProducerName => _producerActor.AskFor<string>(GetProducerName.Instance);

        public long LastSequenceId => _producerActor.AskFor<long>(GetLastSequenceId.Instance);
        
        public IProducerStats Stats => _producerActor.AskFor<IProducerStats>(GetStats.Instance);

        public bool Connected => _producerActor.AskFor<bool>(IsConnected.Instance);

        public long LastDisconnectedTimestamp => _producerActor.AskFor<long>(GetLastDisconnectedTimestamp.Instance);

        public void Close()
        {
            _producerActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public void Flush()
        {
            _producerActor.Tell(Messages.Producer.Flush.Instance);
        }

        public ITypedMessageBuilder<T> NewMessage()
        {
            return new TypedMessageBuilder<T>(_producerActor, _schema);
        }

        public ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
        {
            Condition.CheckArgument(schema != null);
            return new TypedMessageBuilder<V>(_producerActor, schema);
        }

        public TypedMessageBuilder<T> NewMessage(Transaction txn)
        {
            // check the producer has proper settings to send transactional messages
            if (_conf.SendTimeoutMs > 0)
            {
                throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
            }

            return new TypedMessageBuilder<T>(_producerActor, _schema, txn);
        }
        internal IActorRef GetProducer => _producerActor;
        public SentMessage<T> Send(T message)
        {
            NewMessage().Value(message).Send();
            return _queue.SentMessage.Take();
        }
        public SentMessage<T> SendReceipt()
        {
            SentMessage<T> sent = null;
            while (sent == null || (sent.Errored && sent.Exception is NullReferenceException))
            {
                if (_queue.SentMessage.TryTake(out sent, TimeSpan.FromSeconds(5)))
                {

                }
            }
            return sent;
        }
    }
}
