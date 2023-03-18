using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.TransactionImpl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpPulsar
{
    public class Producer<T> : IProducer<T>
    {
        private readonly IActorRef _producerActor;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;
        private readonly TimeSpan _operationTimeout;
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
            => TopicAsync().GetAwaiter().GetResult();
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
        public virtual async ValueTask<bool> ConnectedAsync()
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
            try 
            {
                await _producerActor.Ask<AskResponse>(Messages.Requests.Close.Instance).ConfigureAwait(false);
                await _producerActor.GracefulStop(TimeSpan.FromSeconds(30)).ConfigureAwait(false); 
            }
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

        public ITypedMessageBuilder<T> NewMessage(Transaction txn)
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
        private readonly ConcurrentDictionary<int, Producer<T>> _producers;
        private readonly IActorRef _producerActor;
        private readonly TimeSpan _opTimeout;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;
        public PartitionedProducer(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf, TimeSpan opTimeout, ConcurrentDictionary<int, IActorRef> pr) : base(producer, schema, conf, opTimeout)
        {
            _conf = conf;
            _schema = schema;
            _opTimeout = opTimeout;
            _producers = new ConcurrentDictionary<int, Producer<T>>();
            _producerActor = producer;
            foreach (var s in pr)
            {
                _producers.TryAdd(s.Key, new Producer<T>(s.Value, schema, conf, opTimeout));
            }

        }
        public async ValueTask ToProducers()
        {
            var producer = await _producerActor.Ask<SetProducers>(GetProducers.Instance);
            foreach (var s in producer.Producers)
            {
                _producers.TryAdd(s.Key, new Producer<T>(s.Value, _schema, _conf, _opTimeout));
            }
        }
        public async ValueTask<IList<Producer<T>>> Producers()
        {
            var producer = new List<Producer<T>>();
            foreach (var v in _producers.Values)
            {
                //var y = v.Topic;
                //var e = TopicName.GetPartitionIndex(v.Topic);

                try
                {
                    var e = v.GetProducer;
                    var h = await e.Ask(GetTopic.Instance, TimeSpan.FromSeconds(1));
                    producer.Add(v);
                }
                catch { }

            }
            return producer.OrderBy(e => TopicName.GetPartitionIndex(topic: e.Topic)).ToList();
        }
        public async ValueTask<bool> ConnectedAsync(Producer<bool> producer)
        {
            return await producer.ConnectedAsync();
        }
        public async ValueTask<string> TopicAsync(Producer<bool> producer)
        {
            var c = await producer.TopicAsync();
            return c.ToString();
        }
        public async ValueTask<string> ProducerNameAsync(Producer<bool> producer)
            => await producer.ProducerNameAsync();

        public async ValueTask<long> LastSequenceIdAsync(Producer<bool> producer)
            => await producer.LastSequenceIdAsync();

        public async ValueTask<IProducerStats> StatsAsync(Producer<bool> producer)
            => await producer.StatsAsync();

        public async ValueTask<long> LastDisconnectedTimestampAsync(Producer<bool> producer)
            => await producer.LastDisconnectedTimestampAsync();

        public async ValueTask CloseAsync(Producer<bool> producer)
        {
            await producer.CloseAsync();
        }
        public void Flush(Producer<bool> producer)
        {
            producer.Flush();
        }

    }
}
