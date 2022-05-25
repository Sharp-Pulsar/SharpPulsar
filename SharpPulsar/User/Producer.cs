using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Producer<T> : IProducer<T>
    {
        private readonly IActorRef _producerActor;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;
        private readonly TimeSpan _operationTimeout;
        private readonly string _topic;

        public Producer(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf, TimeSpan opTimeout)
        {
            _producerActor = producer;
            _schema = schema;
            _conf = conf;
            _operationTimeout = opTimeout;
            //_topic = producer.Ask<string>(GetTopic.Instance).Result;
            //var topic = _topic;
        }
        public string Topic 
            => _topic;
        public async ValueTask<string> TopicAsync()
        {
           var c = await _producerActor.Ask<string>(GetTopic.Instance /*, TimeSpan.FromSeconds(2)*/);
           return c.ToString();    
        }

        public string ProducerName 
            => ProducerNameAsync().GetAwaiter().GetResult();
        public async ValueTask<string> ProducerNameAsync()
            => await _producerActor.Ask<string>(GetProducerName.Instance);

        public long LastSequenceId 
            => LastSequenceIdAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastSequenceIdAsync()
            => await _producerActor.Ask<long>(GetLastSequenceId.Instance);
        
        public IProducerStats Stats 
            => StatsAsync().GetAwaiter().GetResult();
        public async ValueTask<IProducerStats> StatsAsync()
            => await _producerActor.Ask<IProducerStats>(GetStats.Instance);

        public bool Connected 
            => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() 
            => await _producerActor.Ask<bool>(IsConnected.Instance);

        public long LastDisconnectedTimestamp 
            => LastDisconnectedTimestampAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastDisconnectedTimestampAsync()
            => await _producerActor.Ask<long>(GetLastDisconnectedTimestamp.Instance);

        public void Close()
        {
            CloseAsync().GetAwaiter().GetResult();
        }
        public async ValueTask CloseAsync()
        {
            try { await _producerActor.GracefulStop(_operationTimeout).ConfigureAwait(false); }
            catch { }
        }
        public void Flush()
        {
            _producerActor.Tell(Messages.Producer.Flush.Instance);
        }

        public ITypedMessageBuilder<T> NewMessage()
        {
            return new TypedMessageBuilder<T>(_producerActor, _schema, _conf);
        }

        public ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
        {
            Condition.CheckArgument(schema != null);
            return new TypedMessageBuilder<V>(_producerActor, schema, _conf);
        }

        public TypedMessageBuilder<T> NewMessage(Transaction txn)
        {
            // check the producer has proper settings to send transactional messages
            if (_conf.SendTimeoutMs.TotalMilliseconds > 0)
            {
                throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
            }

            return new TypedMessageBuilder<T>(_producerActor, _schema, txn, _conf);
        }
        internal IActorRef GetProducer => _producerActor;

        public MessageId Send(T message)
        {
            return SendAsync(message).GetAwaiter().GetResult();
        }
        public async ValueTask<MessageId> SendAsync(T message)
        {
            return await NewMessage().Value(message).SendAsync().ConfigureAwait(false);
        }

        public MessageId Send<TK, TV>(T message)
        {
            return SendAsync<TK, TV>(message).GetAwaiter().GetResult();
        }
        public async ValueTask<MessageId> SendAsync<TK, TV>(T message)
        {
            return await NewMessage().Value<TK, TV>(message).SendAsync().ConfigureAwait(false);
        }
    }
    public class PartitionedProducer<T> : Producer<T>
    {
        private readonly IActorRef _producerActor;
        public PartitionedProducer(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf, TimeSpan opTimeout) : base(producer, schema, conf, opTimeout)
        {
            _producerActor = producer;
        }
        public async ValueTask<IList<Producer<T>>> Producers()
        {
            var producer = await _producerActor.Ask<GetProducers<T>>(SetProducers.Instance);
            return producer.Producers;
        }
    }
}
