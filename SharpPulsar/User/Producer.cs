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
        public readonly IActorRef ProducerActor;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;
        private readonly TimeSpan _operationTimeout;

        public Producer(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf, TimeSpan opTimeout)
        {
            ProducerActor = producer;
            _schema = schema;
            _conf = conf;
            _operationTimeout = opTimeout;
        }
        public string Topic 
            => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() 
            => await ProducerActor.Ask<string>(GetTopic.Instance);

        public string ProducerName 
            => ProducerNameAsync().GetAwaiter().GetResult();
        public async ValueTask<string> ProducerNameAsync()
            => await ProducerActor.Ask<string>(GetProducerName.Instance);

        public long LastSequenceId 
            => LastSequenceIdAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastSequenceIdAsync()
            => await ProducerActor.Ask<long>(GetLastSequenceId.Instance);
        
        public IProducerStats Stats 
            => StatsAsync().GetAwaiter().GetResult();
        public async ValueTask<IProducerStats> StatsAsync()
            => await ProducerActor.Ask<IProducerStats>(GetStats.Instance);

        public bool Connected 
            => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() 
            => await ProducerActor.Ask<bool>(IsConnected.Instance);

        public long LastDisconnectedTimestamp 
            => LastDisconnectedTimestampAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastDisconnectedTimestampAsync()
            => await ProducerActor.Ask<long>(GetLastDisconnectedTimestamp.Instance);

        public void Close()
        {
            CloseAsync().GetAwaiter().GetResult();
        }
        public async ValueTask CloseAsync()
        {
            try { await ProducerActor.GracefulStop(_operationTimeout).ConfigureAwait(false); }
            catch { }
        }
        public void Flush()
        {
            ProducerActor.Tell(Messages.Producer.Flush.Instance);
        }

        public ITypedMessageBuilder<T> NewMessage()
        {
            return new TypedMessageBuilder<T>(ProducerActor, _schema, _conf);
        }

        public ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
        {
            Condition.CheckArgument(schema != null);
            return new TypedMessageBuilder<V>(ProducerActor, schema, _conf);
        }

        public TypedMessageBuilder<T> NewMessage(Transaction txn)
        {
            // check the producer has proper settings to send transactional messages
            if (_conf.SendTimeoutMs.TotalMilliseconds > 0)
            {
                throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
            }

            return new TypedMessageBuilder<T>(ProducerActor, _schema, txn, _conf);
        }
        internal IActorRef GetProducer => ProducerActor;

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
        
        public PartitionedProducer(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf, TimeSpan opTimeout) : base(producer, schema, conf, opTimeout)
        {
            
        }
        public async ValueTask<IList<Producer<T>>> Producers()
        {
            var producer = await ProducerActor.Ask<GetProducers<T>>(SetProducers.Instance);
            return producer.Producers;
        }
    }
}
