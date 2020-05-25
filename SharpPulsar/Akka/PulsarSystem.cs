using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarSystem: IAsyncDisposable
    {
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _pulsarManager;
        private readonly ClientConfigurationData _conf;
        private readonly PulsarManagerState _managerState;

        public PulsarSystem(ClientConfigurationData conf)
        {
            _managerState = new PulsarManagerState
            {
                ConsumerQueue = new BlockingQueue<CreatedConsumer>(),
                ProducerQueue = new BlockingQueue<CreatedProducer>(),
                DataQueue = new BlockingQueue<SqlData>(),
                SchemaQueue = new BlockingQueue<GetOrCreateSchemaServerResponse>(),
                MessageIdQueue =  new BlockingQueue<LastMessageIdReceived>()
            };
            _conf = conf;
            var config = ConfigurationFactory.ParseString(@"
            akka
            {
                loglevel = DEBUG
			    log-config-on-start = on 
                loggers=[""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]
			    actor 
                {              
				      debug 
				      {
					      receive = on
					      autoreceive = on
					      lifecycle = on
					      event-stream = on
					      unhandled = on
				      }  
			    }
                coordinated-shutdown
                {
                    exit-clr = on
                }
            }"
            );
            _actorSystem = ActorSystem.Create("Pulsar", config);
            _pulsarManager = _actorSystem.ActorOf(PulsarManager.Prop(conf, _managerState), "PulsarManager");
        }

        public (IActorRef Producer, string Topic, string ProducerName) PulsarProducer(CreateProducer producer)
        {
            if (producer == null)
                throw new ArgumentNullException(nameof(producer), "null");
            var conf = producer.ProducerConfiguration;

            if (!TopicName.IsValid(conf.TopicName))
                throw new ArgumentException("Topic is invalid");

            var topic = TopicName.Get(conf.TopicName);
            conf.TopicName = topic.ToString();
            var p = new NewProducer(producer.Schema, _conf, conf);
            _pulsarManager.Tell(p);
            if (_managerState.ProducerQueue.TryTake(out var createdProducer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (createdProducer.Producer, createdProducer.Topic, createdProducer.Name);
            }
            throw new TimeoutException($"Timeout waiting for producer creation!");
        }
        
        public (byte[] SchemaVersion, string ErrorCode, string ErrorMessage) PulsarProducer(RegisterSchema schema, IActorRef producer)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema), "null");
            if(schema.Schema == null)
                throw new ArgumentNullException(nameof(schema.Schema), "null");
            if (string.IsNullOrWhiteSpace(schema.Topic))
                throw new ArgumentNullException(nameof(schema.Topic), "null");
            producer.Tell(schema);
            if (_managerState.SchemaQueue.TryTake(out var register, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (register.SchemaVersion, register.ErrorCode.ToString(), register.ErrorMessage);
            }
            throw new TimeoutException($"Timeout waiting for schema registration!");
        }
        public (IActorRef Producer, string Topic, string ProducerName) PulsarProducer(CreateProducerBroadcastGroup producer)
        {
            if (producer == null)
                throw new ArgumentNullException(nameof(producer), "null");
            if(string.IsNullOrWhiteSpace(producer.Title))
                throw new ArgumentException("Title not supplied");
            if(producer.ProducerConfigurations.Count < 2)
                throw new ArgumentNullException(nameof(producer), "numbers of producers must be greater than 1");
            foreach (var t in producer.ProducerConfigurations)
            {
                if (!TopicName.IsValid(t.TopicName))
                    throw new ArgumentException($"Topic '{t.TopicName}' is invalid");
                t.TopicName = TopicName.Get(t.TopicName).ToString();
            }
            var group = new NewProducerBroadcastGroup(producer.Schema, _conf, producer.ProducerConfigurations.ToImmutableHashSet(), producer.Title);
            _pulsarManager.Tell(group);
            if (_managerState.ProducerQueue.TryTake(out var createdProducer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (createdProducer.Producer, createdProducer.Topic, createdProducer.Name);
            }
            throw new TimeoutException($"Timeout waiting for producer creation!");
        }
        public (IActorRef Reader, string Topic) PulsarReader(CreateReader reader)
        {
            if (reader.Seek != null)
            {
                if (reader.Seek.Type == null || reader.Seek.Input == null)
                    throw new ArgumentException("Seek is in an invalid state: null Type or Input");
                switch (reader.Seek.Type)
                {
                    case SeekType.MessageId when !(reader.Seek.Input is string):
                        throw new ArgumentException("SeekType.MessageId requires a string input");
                    case SeekType.Timestamp when !(reader.Seek.Input is long):
                        throw new ArgumentException("SeekType.Timestamp requires a long input");
                    default:
                        throw new ArgumentException($"Seek type '{reader.Seek.Type}' is not supported");
                }
            }
            var p = new NewReader(reader.Schema, _conf, reader.ReaderConfiguration, reader.Seek);
            _pulsarManager.Tell(p);
            if (_managerState.ConsumerQueue.TryTake(out var createdConsumer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (createdConsumer.Consumer, createdConsumer.Topic);
            }
            throw new TimeoutException($"Timeout waiting for reader creation!");
        }

        public IEnumerable<SqlData> PulsarSql(InternalCommands.Sql data)
        {
            if(string.IsNullOrWhiteSpace(data.DestinationServer) || data.ExceptionHandler == null || string.IsNullOrWhiteSpace(data.Query) || data.Log == null)
                throw new ArgumentException("'Sql' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
            var hasRow = true;
            while (hasRow)
            {
                if (_managerState.DataQueue.TryTake(out var sqlData, _conf.OperationTimeoutMs, CancellationToken.None))
                {
                    hasRow = sqlData.HasRow;
                    yield return sqlData;
                }
                else
                {
                    break;
                }
            }
        }
        public void PulsarAdmin(InternalCommands.Admin data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null || data.Handler == null  || data.Log == null)
                throw new ArgumentException("'Admin' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
        }
        public void PulsarFunction(InternalCommands.Function data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null || data.Handler == null  || data.Log == null)
                throw new ArgumentException("'Function' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
        }
        public void PulsarTransaction()
        {
            
        }
        public void SetupSqlServers(SqlServers servers)
        {
            if(servers == null)
                throw new ArgumentNullException(nameof(servers), "null");
            if (servers.Servers.Count < 1)
                throw new ArgumentException("SqlServers is in an invalid state: Servers must be greater than 1");
            _pulsarManager.Tell(servers);
        }
        public (IActorRef Consumer, string Topic) PulsarConsumer(CreateConsumer consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");
            if (consumer.ConsumerType == ConsumerType.Multi)
            {
                if (consumer.ConsumerConfiguration.TopicNames.Count < 1)
                {
                   throw new ArgumentException("To Create Multi Topic Consumers, Topics must be greater than 1");
                }

                if (!TopicNamesValid(consumer.ConsumerConfiguration.TopicNames))
                {
                    throw new ArgumentException("Topics should have same namespace.");
                }
            }

            if (!consumer.ConsumerConfiguration.TopicNames.Any() && consumer.ConsumerConfiguration.TopicsPattern == null)
                throw new ArgumentException("Please set topic(s) or topic pattern");
            if (consumer.Seek != null)
            {
                if (consumer.Seek.Type == null || consumer.Seek.Input == null)
                    throw new ArgumentException("Seek is in an invalid state: null Type or Input");
                switch (consumer.Seek.Type)
                {
                    case SeekType.MessageId when !(consumer.Seek.Input is string):
                        throw new ArgumentException("SeekType.MessageId requires a string input");
                    case SeekType.Timestamp when !(consumer.Seek.Input is long):
                        throw new ArgumentException("SeekType.Timestamp requires a long input");
                }
            }
            var c = new NewConsumer(consumer.Schema, _conf, consumer.ConsumerConfiguration, consumer.ConsumerType, consumer.Seek);
            _pulsarManager.Tell(c);
            if (_managerState.ConsumerQueue.TryTake(out var createdConsumer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (createdConsumer.Consumer, createdConsumer.Topic);
            }
            throw new TimeoutException($"Timeout waiting for consumer creation!");
        }
        public void PulsarConsumer(RedeliverMessages messages, IActorRef consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");
            if (messages == null)
                throw new ArgumentException("RedeliverMessages is null");
            consumer.Tell(messages);
        }
        public (string Topic, long LedgerId, long EntryId, int Partition, int BatchIndex) PulsarConsumer(LastMessageId last, IActorRef consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");
            consumer.Tell(last);
            if (_managerState.MessageIdQueue.TryTake(out var messageId, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return (messageId.Topic, messageId.Response.LedgerId, messageId.Response.EntryId, messageId.Response.Partition, messageId.Response.BatchIndex);
            }
            throw new TimeoutException($"Timeout waiting for last message id!");
        }
        public void PulsarConsumer(Seek seek, IActorRef consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");
            if (seek == null)
                throw new ArgumentException("Seek is null");
            consumer.Tell(seek);
        }
        public void PulsarReader(RedeliverMessages messages, IActorRef reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "Null");
            if (messages == null)
                throw new ArgumentException("RedeliverMessages is null");
            reader.Tell(messages);
        }
        public void PulsarReader(Seek seek, IActorRef reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "Null");
            if (seek == null)
                throw new ArgumentException("Seek is null");
            reader.Tell(seek);
        }
        public void Send(Send send, IActorRef producer)
        {
           producer.Tell(send);
        }
        public void BulkSend(BulkSend send, IActorRef producer)
        {
            producer.Tell(send);
        }
        public async ValueTask DisposeAsync()
        {
           await _actorSystem.Terminate();
        }
        // Check topics are valid.
        // - each topic is valid,
        // - every topic has same namespace,
        // - topic names are unique.
        private static bool TopicNamesValid(ICollection<string> topics)
        {
            var @namespace = TopicName.Get(topics.First()).Namespace;
            var result = topics.FirstOrDefault(topic => TopicName.IsValid(topic) && @namespace.Equals(TopicName.Get(topic).Namespace));

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentException($"Received invalid topic name: {string.Join("; ", result) }");
            }

            // check topic names are unique
            var set = new HashSet<string>(topics);
            if (set.Count == topics.Count)
            {
                return true;
            }
            throw new ArgumentException($"Topic names not unique. unique/all : {set.Count}/{topics.Count}");

        }
    }
}
