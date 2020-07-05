using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.Function;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Akka.Sql.Live;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using TopicEntries = SharpPulsar.Akka.InternalCommands.Consumer.TopicEntries;

namespace SharpPulsar.Akka
{
    public sealed class PulsarSystem
    {
        private static PulsarSystem _instance;
        private static readonly object Lock = new object();
        private readonly SystemMode _systemMode;
        private readonly TestObject _testObject;
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _pulsarManager;
        private readonly ClientConfigurationData _conf;
        private readonly PulsarManagerState _managerState;
        private readonly Dictionary<string, CommandSubscribe.SubType> _topicSubTypes;
        public static PulsarSystem GetInstance(ActorSystem actorSystem, ClientConfigurationData conf, SystemMode mode = SystemMode.Normal)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(actorSystem, conf, mode);
                    }
                }
            }
            return _instance;
        }
        public static PulsarSystem GetInstance(ClientConfigurationData conf, SystemMode mode = SystemMode.Normal)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(conf, mode);
                    }
                }
            }
            return _instance;
        }
        private PulsarSystem(ActorSystem actorSystem, ClientConfigurationData conf, SystemMode mode)
        {
            _topicSubTypes = new Dictionary<string, CommandSubscribe.SubType>();
            _testObject = new TestObject {ActorSystem = actorSystem};
            _systemMode = mode;
            _actorSystem = actorSystem;
            _managerState = new PulsarManagerState
            {
                ConsumerQueue = new BlockingQueue<CreatedConsumer>(),
                ProducerQueue = new BlockingQueue<CreatedProducer>(),
                DataQueue = new BlockingQueue<SqlData>(),
                SchemaQueue = new BlockingQueue<GetOrCreateSchemaServerResponse>(),
                MessageIdQueue =  new BlockingQueue<LastMessageIdReceived>(),
                LiveDataQueue = new BlockingCollection<LiveSqlData>(),
                MessageQueue = new ConcurrentDictionary<string, BlockingCollection<ConsumedMessage>>(),
                EventQueue = new BlockingQueue<IEventMessage>(),
                MaxQueue = new BlockingQueue<TopicEntries>(),
                AdminQueue = new BlockingQueue<AdminResponse>(),
                FunctionQueue = new BlockingQueue<FunctionResponse>()
            };
            _conf = conf;
            _pulsarManager = _actorSystem.ActorOf(PulsarManager.Prop(conf, _managerState), "PulsarManager");
        }
        private PulsarSystem(ClientConfigurationData conf, SystemMode mode)
        {
            _testObject = new TestObject();
            _systemMode = mode;
            _topicSubTypes = new Dictionary<string, CommandSubscribe.SubType>();
            _managerState = new PulsarManagerState
            {
                ConsumerQueue = new BlockingQueue<CreatedConsumer>(),
                ProducerQueue = new BlockingQueue<CreatedProducer>(),
                DataQueue = new BlockingQueue<SqlData>(),
                SchemaQueue = new BlockingQueue<GetOrCreateSchemaServerResponse>(),
                MessageIdQueue =  new BlockingQueue<LastMessageIdReceived>(),
                LiveDataQueue = new BlockingCollection<LiveSqlData>(),
                MessageQueue =  new ConcurrentDictionary<string, BlockingCollection<ConsumedMessage>>(),
                EventQueue = new BlockingQueue<IEventMessage>(),
                MaxQueue = new BlockingQueue<TopicEntries>(),
                AdminQueue = new BlockingQueue<AdminResponse>(),
                FunctionQueue = new BlockingQueue<FunctionResponse>()
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
            _testObject.ActorSystem = _pulsarManager.Ask<ActorSystem>("ActorSystem").GetAwaiter().GetResult();
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
                if (_systemMode == SystemMode.Test)
                    _testObject.Producer = createdProducer.Producer;
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
            if (!TopicName.IsValid(schema.Topic))
                throw new ArgumentException("Topic is invalid");

            var topic = TopicName.Get(schema.Topic);
            producer.Tell(new RegisterSchema(schema.Schema, topic.ToString()));
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
            var originalName = reader.ReaderConfiguration.ReaderName;

            var name = Regex.Replace(originalName, @"[^\w\d]", "");

            if (_managerState.MessageQueue.Any(x => x.Key.StartsWith(name)))
                throw new ArgumentException($"consumer with name '{originalName}'");

            reader.ReaderConfiguration.ReaderName = name;

            if (string.IsNullOrWhiteSpace(reader.ReaderConfiguration.ReaderName))
                throw new ArgumentException("Reader name is required");

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
                if (_systemMode == SystemMode.Test)
                    _testObject.Consumer = createdConsumer.Consumer;
                return (createdConsumer.Consumer, createdConsumer.Topic);
            }
            throw new TimeoutException($"Timeout waiting for reader creation!");
        }

        public IEnumerable<SqlData> PulsarSql(InternalCommands.Sql data)
        {
            if(string.IsNullOrWhiteSpace(data.Server) || data.ExceptionHandler == null || string.IsNullOrWhiteSpace(data.Query) || data.Log == null)
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
        public IEnumerable<LiveSqlData> PulsarSql(LiveSql data)
        {
            if (string.IsNullOrWhiteSpace(data.Server) || data.ExceptionHandler == null || string.IsNullOrWhiteSpace(data.Command) || data.Log == null || string.IsNullOrWhiteSpace(data.Topic))
                throw new ArgumentException("'Sql' is in an invalid state: null field not allowed");
            if (!data.Command.Contains("__publish_time__ > {time}"))
            {
                if (data.Command.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("add '__publish_time__ > {time}' to where clause");
                }
                throw new ArgumentException("add 'where __publish_time__ > {time}' to '"+data.Command+"'");
            }
            if(!TopicName.IsValid(data.Topic))
                throw new ArgumentException($"Topic '{data.Topic}' failed validation");

            _pulsarManager.Tell(new LiveSql(data.Command, data.Frequency, data.StartAtPublishTime, TopicName.Get(data.Topic).ToString(), data.Server, data.Log, data.ExceptionHandler));
            
            foreach (var liveData in _managerState.LiveDataQueue.GetConsumingEnumerable())
            {
                yield return liveData;
            }
        }
        public void PulsarAdmin(InternalCommands.Admin data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null || data.Handler == null  || data.Log == null)
                throw new ArgumentException("'Admin' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
        }
        /// <summary>
        /// To return a data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T PulsarAdmin<T>(InternalCommands.Admin data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null  || data.Log == null)
                throw new ArgumentException("'Admin' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
            if (!_managerState.AdminQueue.TryTake(out var response, _conf.OperationTimeoutMs, CancellationToken.None))
                return default(T);
            //check for exception and null
            if (!(response.Response is Exception) || response.Response != null)
                return (T) response.Response;
            if (response.Response is Exception ex)
                throw ex;
            throw new NullReferenceException();

        }
        /// <summary>
        /// Consume messages from queue. ConsumptionType has to be set to Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="autoAck"></param>
        /// <param name="takeCount"></param>
        /// <param name="customProcess"></param>
        /// <returns></returns>
        public IEnumerable<T> Messages<T>(string consumerName, bool autoAck = true, int takeCount = -1, Func<ConsumedMessage, T> customHander = null)
        {
            if (customHander == null)
            {
                if (takeCount == -1)
                {
                    foreach (var m in _managerState.MessageQueue[consumerName].GetConsumingEnumerable())
                    {
                        var received = m.Message.ToTypeOf<T>();
                        if (autoAck)
                        {
                            if (m.Message.MessageId is MessageId mi)
                            {
                                m.Consumer.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, m.AckSets.ToArray())));
                            }
                            else if (m.Message.MessageId is BatchMessageId b)
                            {
                                m.Consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, m.AckSets.ToArray())));
                            }
                        }
                        yield return received;
                    }
                }
                else if(takeCount > 0)
                {
                    var takes = 0;
                    while (takes < takeCount)
                    {
                        if (_managerState.MessageQueue[consumerName].TryTake(out var m, _conf.OperationTimeoutMs, CancellationToken.None))
                        {
                            var received = m.Message.ToTypeOf<T>();
                            if (autoAck)
                            {
                                if (m.Message.MessageId is MessageId mi)
                                {
                                    m.Consumer.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, m.AckSets.ToArray())));
                                }
                                else if (m.Message.MessageId is BatchMessageId b)
                                {
                                    m.Consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, m.AckSets.ToArray())));
                                }
                            }
                            yield return received;
                            takes++;
                        }
                    }
                }
            }
            else
            {
                if (takeCount == -1)
                {
                    foreach (var m in _managerState.MessageQueue[consumerName].GetConsumingEnumerable())
                    {
                        if (autoAck)
                        {
                            if (m.Message.MessageId is MessageId mi)
                            {
                                m.Consumer.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, m.AckSets.ToArray())));
                            }
                            else if (m.Message.MessageId is BatchMessageId b)
                            {
                                m.Consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, m.AckSets.ToArray())));
                            }
                        }
                        yield return customHander(m);
                    }
                }
                else if(takeCount > 0)
                {
                    var takes = 0;
                    while (takes < takeCount)
                    {
                        if (_managerState.MessageQueue[consumerName].TryTake(out var m, _conf.OperationTimeoutMs, CancellationToken.None))
                        {
                            if (autoAck)
                            {
                                if (m.Message.MessageId is MessageId mi)
                                {
                                    m.Consumer.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, m.AckSets.ToArray())));
                                }
                                else if (m.Message.MessageId is BatchMessageId b)
                                {
                                    m.Consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, m.AckSets.ToArray())));
                                }
                            }
                            yield return customHander(m);
                            takes++;
                        }
                    }
                }
            }
        }
        public ConsumedMessage Receive(string consumerName, int receiveTimeout = 3000)
        {
            if (_managerState.MessageQueue[consumerName].TryTake(out var m, receiveTimeout, CancellationToken.None))
            {
                return m;
            }

            return null;
        }

        public void Acknowledge(ConsumedMessage m)
        {
            if (m.Message.MessageId is MessageId mi)
            {
                m.Consumer.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, m.AckSets.ToArray())));
            }
            else if (m.Message.MessageId is BatchMessageId b)
            {
                m.Consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, m.AckSets.ToArray())));
            }
        }

        /// <summary>
        /// Cumulative ack only on exclusive subscription
        /// </summary>
        /// <param name="ackMessages"></param>
        /// <param name="consumer"></param>
        public void AcknowledgeCumulative(ConsumedMessage m)
        {
            if (m.Message.MessageId is MessageId mi)
            {
                m.Consumer.Tell(new AckMessages(mi, m.AckSets.ToArray()));
            }
            else if (m.Message.MessageId is BatchMessageId b)
            {
                m.Consumer.Tell(new AckMessages(new MessageId(b.LedgerId, b.EntryId, b.PartitionIndex), m.AckSets.ToArray()));
            }
        }

        public void NegativeAcknowledge(ConsumedMessage message)
        {
            message.Consumer.Tell(new NegativeAck(message.Message.MessageId));
        }
        public void PulsarFunction(InternalCommands.Function data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null || data.Handler == null  || data.Log == null)
                throw new ArgumentException("'Function' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
        }
        public T PulsarFunction<T>(InternalCommands.Function data)
        {
            if (string.IsNullOrWhiteSpace(data.BrokerDestinationUrl) || data.Exception == null || data.Handler == null  || data.Log == null)
                throw new ArgumentException("'Function' is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
            if (!_managerState.FunctionQueue.TryTake(out var response, _conf.OperationTimeoutMs, CancellationToken.None))
                return default(T);
            //check for exception and null
            if (!(response.Response is Exception) || response.Response != null)
                return (T)response.Response;
            if (response.Response is Exception ex)
                throw ex;
            throw new NullReferenceException();
        }
        public void PulsarTransaction()
        {
            
        }
        public (IActorRef Consumer, string Topic) PulsarConsumer(CreateConsumer consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");

            var topic = consumer.ConsumerConfiguration.SingleTopic;

            if (!TopicName.IsValid(topic))
                throw new ArgumentException($"Topic '{topic}' is invalid");

            topic = TopicName.Get(topic).ToString();
            /*if (_topicSubTypes.ContainsKey(topic))
            {
                var sub = _topicSubTypes[topic];
                if(sub == CommandSubscribe.SubType.Exclusive)
                    throw new ArgumentException($"{topic} has an exclusive subscriber");

            }*/
            var subType = consumer.ConsumerConfiguration.SubscriptionType;

            consumer.ConsumerConfiguration.SingleTopic = topic;

            var originalName = consumer.ConsumerConfiguration.ConsumerName;


            var name = Regex.Replace(originalName, @"[^\w\d]", "");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Consumer Name is required!");

            if (_managerState.MessageQueue.Any(x => x.Key.StartsWith(name)))
                throw new ArgumentException($"consumer with name '{originalName}'");

            consumer.ConsumerConfiguration.ConsumerName = name;

            if (string.IsNullOrWhiteSpace(consumer.ConsumerConfiguration.SingleTopic))
                throw new ArgumentException("Topic cannot be empty");

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
            var c = new NewConsumer(consumer.Schema, _conf, consumer.ConsumerConfiguration, ConsumerType.Single, consumer.Seek);
            _pulsarManager.Tell(c);
            if (_managerState.ConsumerQueue.TryTake(out var createdConsumer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                if (_systemMode == SystemMode.Test)
                    _testObject.Consumer = createdConsumer.Consumer;

                _managerState.MessageQueue.TryAdd(createdConsumer.ConsumerName, new BlockingCollection<ConsumedMessage>());
                _topicSubTypes.Add(createdConsumer.Topic, subType);
                return (createdConsumer.Consumer, createdConsumer.Topic);
            }
            throw new TimeoutException($"Timeout waiting for consumer creation!");
        }

        public IEnumerable<(IActorRef Consumer, string Topic)> PulsarConsumer(CreateMultiConsumer consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer), "null");

            var type = ConsumerType.Multi;

            var originalName = consumer.ConsumerConfiguration.ConsumerName;
            
            var name = Regex.Replace(originalName, @"[^\w\d]", "");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Consumer Name is required!");

            if (_managerState.MessageQueue.Any(x => x.Key.StartsWith(name)))
                throw new ArgumentException($"consumer with name '{originalName}'");

            consumer.ConsumerConfiguration.ConsumerName = name;

            if (consumer.ConsumerConfiguration.TopicsPattern == null)
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
            else type = ConsumerType.Pattern;

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
            var c = new NewConsumer(consumer.Schema, _conf, consumer.ConsumerConfiguration, type, consumer.Seek);
            _pulsarManager.Tell(c);
            var gotten = false;
            if (_managerState.ConsumerQueue.TryTake(out var createdConsumer, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                if (_systemMode == SystemMode.Test)
                    _testObject.Consumer = createdConsumer.Consumer;

                _managerState.MessageQueue.TryAdd(createdConsumer.ConsumerName, new BlockingCollection<ConsumedMessage>());

                yield return (createdConsumer.Consumer, createdConsumer.Topic);
                gotten = true;
            }
            if(!gotten)
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
        public TopicEntries EventSource(GetNumberOfEntries entries)
        {
            if(entries == null)
                throw new ArgumentException($"GetNumberOfEntries is null");
            if (!TopicName.IsValid(entries.Topic))
                throw new ArgumentException($"Topic '{entries.Topic}' is invalid");
            var topic = TopicName.Get(entries.Topic).ToString();

            _pulsarManager.Tell(new GetNumberOfEntries(topic, entries.Server));
            if (_managerState.MaxQueue.TryTake(out var msg, _conf.OperationTimeoutMs, CancellationToken.None))
            {
                return msg;
            }

            throw new TimeoutException("Timeout waiting for Entries");
        }
        /// <summary>
        /// if you are supplying custom handler, IEventMessage is either EventMessage - when a message has a given property or NotTagged - when a message does not have a given property.
        /// You are responsible for determining the message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="replay"></param>
        /// <param name="customHandler"></param>
        /// <returns>T</returns>
        public IEnumerable<T> EventSource<T>(NextPlay replay, Func<IEventMessage, T> customHandler = null)
        {
            if(replay == null)
                throw new ArgumentException($"ReplayTopic is null");
            if (!TopicName.IsValid(replay.Topic))
                throw new ArgumentException($"Topic '{replay.Topic}' is invalid");
            var topic = TopicName.Get(replay.Topic).ToString();

            var @from = replay.From > 0 ? replay.From : 1;
            var max = Math.Min(replay.To, replay.Max);
            var takeCount = max - replay.From;

            _pulsarManager.Tell(new NextPlay(topic, max, @from, replay.To, replay.Tagged));
            var count = 1;
            if (customHandler == null)
            {
                while (takeCount >= count)
                {
                    count++;
                    if (_managerState.EventQueue.TryTake(out var msg, _conf.OperationTimeoutMs, CancellationToken.None))
                    {
                        if (msg is EventMessage evt)
                        {
                            yield return evt.Message.ToTypeOf<T>();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (takeCount >= count)
                {
                    count++;
                    if (_managerState.EventQueue.TryTake(out var msg, _conf.OperationTimeoutMs, CancellationToken.None))
                    {
                        //Client should be responsible for checking if message is EventMessage or NotTagged
                        yield return customHandler(msg);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// if you are supplying custom handler, IEventMessage is either EventMessage - when a message has a given property or NotTagged - when a message does not have a given property.
        /// You are responsible for determining the message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="replay"></param>
        /// <param name="customHandler"></param>
        /// <returns>T</returns>
        public IEnumerable<T> EventSource<T>(ReplayTopic replay, Func<IEventMessage, T> customHandler = null)
        {
            if(replay == null)
                throw new ArgumentException($"ReplayTopic is null");
            
            if(replay.ReaderConfigurationData == null)
                throw new ArgumentException($"ReaderConfigurationData is null");
            
            if(string.IsNullOrWhiteSpace(replay.AdminUrl))
                throw new ArgumentException($"AdminUrl cannot be empty");

            var topic = replay.ReaderConfigurationData.TopicName;

            if (!TopicName.IsValid(replay.ReaderConfigurationData.TopicName))
                throw new ArgumentException($"Topic '{topic}' is invalid");

            replay.ReaderConfigurationData.TopicName = TopicName.Get(topic).ToString();

            if(replay.Tagged && replay.Tag == null)
                throw new ArgumentException($"Tag is null");

            if(replay.Tagged && !replay.ReaderConfigurationData.TopicName.EndsWith("*"))
                throw new ArgumentException($"Topic should end with *");

            var @from = replay.From > 0 ? replay.From : 1;


            var max = Math.Min(replay.To, replay.Max);
            var takeCount = max - replay.From;

            var start = new StartReplayTopic(_conf, replay.ReaderConfigurationData, replay.AdminUrl, @from, replay.To, max, replay.Tag, replay.Tagged);
            
            _pulsarManager.Tell(start);

            var count = 1;
            if (customHandler == null)
            {

                while (takeCount >= count)
                {
                    count++;
                    if (_managerState.EventQueue.TryTake(out var msg, _conf.OperationTimeoutMs, CancellationToken.None))
                    {
                        if (msg is EventMessage evt)
                        {
                            yield return evt.Message.ToTypeOf<T>();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (takeCount >= count)
                {
                    count++;
                    if (_managerState.EventQueue.TryTake(out var msg, _conf.OperationTimeoutMs, CancellationToken.None))
                    {
                        yield return customHandler(msg);
                    }
                    else
                    {
                        break;
                    }
                }
            }
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
        public void Stop()
        {
           _actorSystem.Terminate();
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

        public TestObject GeTestObject()
        {
            if (_testObject.Producer == null || _testObject.Consumer == null)
                throw new Exception("System not ready yet or not in Test mode");
            _testObject.ProducerBroker =
                _testObject.Producer.Ask<IActorRef>(new INeedBroker()).GetAwaiter().GetResult();
            _testObject.ConsumerBroker =
                _testObject.Consumer.Ask<IActorRef>(new INeedBroker()).GetAwaiter().GetResult();
            return _testObject;
        }
    }
    //I need this for AcknowledgementsGroupingTrackerTest etc
    public sealed class TestObject
    {
        public ActorSystem ActorSystem { get; set; }
        public IActorRef Consumer { get; set; }
        public IActorRef Producer { get; set; }
        public IActorRef ConsumerBroker { get; set; }// Ask for it with INeedBroker
        public IActorRef ProducerBroker { get; set; }// Ask for it with INeedBroker
    }
    public enum SystemMode
    {
        Normal,
        Test // for unit testing
    }
}
