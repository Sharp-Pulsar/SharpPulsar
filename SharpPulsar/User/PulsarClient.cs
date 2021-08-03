using Akka.Actor;
using Akka.Event;
using Akka.Util;
using SharpPulsar.Cache;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Schema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Transaction;
using SharpPulsar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.User
{
    public class PulsarClient : IPulsarClient
    {
        private readonly IActorRef _client;
        private readonly IActorRef _transactionCoordinatorClient;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly ActorSystem _actorSystem;
        private readonly Cache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = new Cache<string, ISchemaInfoProvider>(TimeSpan.FromMinutes(30), 100000);
        private ILoggingAdapter _log;
        private IActorRef _cnxPool;
        private IActorRef _lookup;
        private IActorRef _generator;

        public PulsarClient(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ClientConfigurationData clientConfiguration, ActorSystem actorSystem, IActorRef transactionCoordinatorClient)
        {
            _generator = idGenerator;
            _client = client;
            _clientConfigurationData = clientConfiguration;
            _actorSystem = actorSystem;
            _transactionCoordinatorClient = transactionCoordinatorClient;
            _log = actorSystem.Log;
            _lookup = lookup;
            _cnxPool = cnxPool;
        }

        public void ReloadLookUp()
        {
            _actorSystem.Stop(_lookup);
            _lookup =_actorSystem.ActorOf(BinaryProtoLookupService.Prop(_cnxPool, _generator, _clientConfigurationData.ServiceUrl, _clientConfigurationData.ListenerName, _clientConfigurationData.UseTls, _clientConfigurationData.MaxLookupRequest, _clientConfigurationData.OperationTimeoutMs), "BinaryProtoLookupService");
            _lookup.Tell(new SetClient(_client));
        }

        public IList<string> GetPartitionsForTopic(string topic)
        {
            throw new NotImplementedException();
        }
        private ISchemaInfoProvider NewSchemaProvider(string topicName)
        {
            return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), _actorSystem.Log, _lookup);
        }
        private async ValueTask<ISchema<T>> PreProcessSchemaBeforeSubscribe<T>(ISchema<T> schema, string topicName)
        {
            if (schema != null && schema.SupportSchemaVersioning())
            {
                ISchemaInfoProvider schemaInfoProvider;
                try
                {
                    schemaInfoProvider = _schemaProviderLoadingCache.Get(topicName);
                    if (schemaInfoProvider == null)
                        _schemaProviderLoadingCache.Put(topicName, NewSchemaProvider(topicName));
                }
                catch (Exception e)
                {
                    _log.Error($"Failed to load schema info provider for topic {topicName}: {e}");
                    throw e;
                }
                schema = schema.Clone();
                if (schema.RequireFetchingSchemaInfo())
                {
                    var finalSchema = schema;
                    var schemaInfo = await schemaInfoProvider.LatestSchema().ConfigureAwait(false);
                    if (null == schemaInfo)
                    {
                        if (!(finalSchema is AutoConsumeSchema))
                        {
                            throw new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName);
                        }
                    }
                    _log.Info($"Configuring schema for topic {topicName} : {schemaInfo}");
                    finalSchema.ConfigureSchemaInfo(topicName, "topic", schemaInfo);
                    finalSchema.SchemaInfoProvider = schemaInfoProvider;
                    return finalSchema;
                }
                else
                {
                    schema.SchemaInfoProvider = schemaInfoProvider;
                }
            }
            return schema;
        }
        public Consumer<byte[]> NewConsumer(ConsumerConfigBuilder<byte[]> conf)
        {
            return NewConsumerAsync(ISchema<byte[]>.Bytes, conf).GetAwaiter().GetResult();
        }
        public async ValueTask<Consumer<byte[]>> NewConsumerAsync(ConsumerConfigBuilder<byte[]> conf)
        {
            return await NewConsumerAsync(ISchema<byte[]>.Bytes, conf).ConfigureAwait(false);
        }
        public Consumer<T> NewConsumer<T>(ISchema<T> schema, ConsumerConfigBuilder<T> confBuilder)
        {
            return NewConsumerAsync(schema, confBuilder).GetAwaiter().GetResult();
        }

        public async ValueTask<Consumer<T>> NewConsumerAsync<T>(ISchema<T> schema, ConsumerConfigBuilder<T> confBuilder)
        {
            var conf = confBuilder.ConsumerConfigurationData;
            // DLQ only supports non-ordered subscriptions, don't enable DLQ on Key_Shared subType since it require message ordering for given key.
            if (conf.SubscriptionType == SubType.KeyShared && !string.IsNullOrWhiteSpace(conf.DeadLetterPolicy?.DeadLetterTopic))
            {
                throw new PulsarClientException.InvalidConfigurationException("Deadletter topic on Key_Shared " +
                        "subscription type is not supported.");
            }
            if (conf.TopicNames.Count == 0 && conf.TopicsPattern == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Topic name must be set on the consumer builder");
            }

            if (string.IsNullOrWhiteSpace(conf.SubscriptionName))
            {
                throw new PulsarClientException.InvalidConfigurationException("Subscription name must be set on the consumer builder");
            }

            if (conf.KeySharedPolicy != null && conf.SubscriptionType != CommandSubscribe.SubType.KeyShared)
            {
                throw new PulsarClientException.InvalidConfigurationException("KeySharedPolicy must set with KeyShared subscription");
            }
            if (conf.RetryEnable && conf.TopicNames.Count > 0)
            {
                var topicFirst = TopicName.Get(conf.TopicNames.GetEnumerator().Current);
                var retryLetterTopic = topicFirst.Namespace + "/" + conf.SubscriptionName + RetryMessageUtil.RetryGroupTopicSuffix;
                var deadLetterTopic = topicFirst.Namespace + "/" + conf.SubscriptionName + RetryMessageUtil.DlqGroupTopicSuffix;
                if (conf.DeadLetterPolicy == null)
                {
                    conf.DeadLetterPolicy = new DeadLetterPolicy
                    {
                        MaxRedeliverCount = RetryMessageUtil.MaxReconsumetimes,
                        RetryLetterTopic = retryLetterTopic,
                        DeadLetterTopic = deadLetterTopic
                    };
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.RetryLetterTopic))
                    {
                        conf.DeadLetterPolicy.RetryLetterTopic = retryLetterTopic;
                    }
                    if (string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
                    {
                        conf.DeadLetterPolicy.DeadLetterTopic = deadLetterTopic;
                    }
                }
                conf.TopicNames.Add(conf.DeadLetterPolicy.RetryLetterTopic);
            }
            return await Subscribe(conf, schema).ConfigureAwait(false);
        }
        
        private async ValueTask<Consumer<T>> Subscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            var state = await _client.Ask<int>(GetClientState.Instance).ConfigureAwait(false);
            if (state != 0)
            {
                throw new PulsarClientException.AlreadyClosedException("Client already closed");
            }

            if (conf == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined");
            }

            foreach (var topic in conf.TopicNames)
            {
                if (!TopicName.IsValid(topic))
                {
                    throw new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'");
                }
            }

            if (string.IsNullOrWhiteSpace(conf.SubscriptionName))
            {
                throw new PulsarClientException.InvalidConfigurationException("Empty subscription name");
            }

            if (conf.ReadCompacted && (conf.TopicNames.Any(topic => TopicName.Get(topic).Domain != TopicDomain.Persistent) || (conf.SubscriptionType != SubType.Exclusive && conf.SubscriptionType != SubType.Failover)))
            {
                throw new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive or failover persistent subscriptions");
            }

            if (conf.ConsumerEventListener != null && conf.SubscriptionType != SubType.Failover)
            {
                throw new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription");
            }

            if (conf.TopicsPattern != null)
            {
                // If use topicsPattern, we should not use topic(), and topics() method.
                if (conf.TopicNames.Count > 0)
                {
                    throw new ArgumentException("Topic names list must be null when use topicsPattern");
                }
                return await PatternTopicSubscribe(conf, schema).ConfigureAwait(false);
            }
            else if (conf.TopicNames.Count == 1)
            {
                return await SingleTopicSubscribe(conf, schema).ConfigureAwait(false);
            }
            else
            {
                return await MultiTopicSubscribe(conf, schema).ConfigureAwait(false);
            }
        }

        private async ValueTask<Consumer<T>> SingleTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            var schemaClone = await PreProcessSchemaBeforeSubscribe(schema, conf.SingleTopic).ConfigureAwait(false);
            return await DoSingleTopicSubscribe(conf, schemaClone).ConfigureAwait(false);
        }

        private async ValueTask<Consumer<T>> DoSingleTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            var topic = conf.SingleTopic;
            try
            {
                var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                IActorRef consumer;
                var state = _actorSystem.ActorOf(Props.Create(()=> new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
                if (metadata.Partitions > 0)
                {
                    Condition.CheckArgument(conf.TopicNames.Count == 1, "Should have only 1 topic for partitioned consumer");

                    // get topic name, then remove it from conf, so constructor will create a consumer with no topic.
                    var cloneConf = conf;
                    var topicName = cloneConf.SingleTopic;
                    cloneConf.TopicNames.Remove(topicName);
                    consumer = _actorSystem.ActorOf(Props.Create<MultiTopicsConsumer<T>>(state, _client, _lookup, _cnxPool, _generator, topicName, conf, _actorSystem.Scheduler.Advanced, schema, true, _clientConfigurationData), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
                    var response = await consumer.Ask<AskResponse>(new Subscribe(topic, metadata.Partitions)).ConfigureAwait(false);
                    if (response.Failed)
                        throw response.Exception;


                    _client.Tell(new AddConsumer(consumer));

                    return new Consumer<T>(state, consumer, schema, conf);
                }
                else
                {
                    var consumerId = await _generator.Ask<long>(NewConsumerId.Instance).ConfigureAwait(false);
                    var partitionIndex = TopicName.GetPartitionIndex(topic);
                    consumer = _actorSystem.ActorOf(Props.Create(()=> new ConsumerActor<T>(consumerId, state, _client, _lookup, _cnxPool, _generator, topic, conf, _actorSystem.Scheduler.Advanced, partitionIndex, false, null, schema, true, _clientConfigurationData)));
                    var response = await consumer.Ask<AskResponse>(Connect.Instance).ConfigureAwait(false);
                    if (response.Failed)
                        throw response.Exception;
                    _client.Tell(new AddConsumer(consumer));
                    return new Consumer<T>(state, consumer, schema, conf);
                }
            }
            catch(Exception e)
            {
                _actorSystem.Log.Error($"[{topic}] Failed to get partitioned topic metadata: {e}");
                throw;
            }
        }

        private async ValueTask<Consumer<T>> MultiTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            Condition.CheckArgument(conf.TopicNames.Count == 0 || TopicNamesValid(conf.TopicNames), "Topics is empty or invalid.");

            var state = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
            var consumer = _actorSystem.ActorOf(Props.Create(()=> new MultiTopicsConsumer<T>(state, _client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, conf.ForceTopicCreation, _clientConfigurationData)), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
            var response = await consumer.Ask<AskResponse>(new SubscribeAndCreateTopicsIfDoesNotExist(conf.TopicNames.ToList(), true)).ConfigureAwait(false);

            if (response.Failed)
                throw response.Exception;

            _client.Tell(new AddConsumer(consumer));

            return new Consumer<T>(state, consumer, schema, conf);
        }

        private async ValueTask<Consumer<T>> PatternTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            var regex = conf.TopicsPattern.ToString();
            var subscriptionMode = ConvertRegexSubscriptionMode(conf.RegexSubscriptionMode);
            var destination = TopicName.Get(regex);
            var namespaceName = destination.NamespaceObject;
            try
            {
                var ask = await _lookup.Ask<AskResponse>(new GetTopicsUnderNamespace(namespaceName, subscriptionMode.Value)).ConfigureAwait(false);
                if (ask.Failed)
                    throw ask.Exception;

                var result = ask.ConvertTo<GetTopicsUnderNamespaceResponse>();
                var topics = result.Topics;
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"Get topics under namespace {namespaceName}, topics.size: {topics.Count}");
                    topics.ForEach(topicName => _actorSystem.Log.Debug($"Get topics under namespace {namespaceName}, topic: {topicName}"));
                }
                var topicsList = TopicsPatternFilter(topics, conf.TopicsPattern);
                topicsList.ToList().ForEach(x => conf.TopicNames.Add(x));
                var state = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
                
                var consumer = _actorSystem.ActorOf(PatternMultiTopicsConsumer<T>.Prop(conf.TopicsPattern, state, _client, _lookup, _cnxPool, _generator, conf, schema, subscriptionMode.Value,  _clientConfigurationData), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
                
                _client.Tell(new AddConsumer(consumer));

                return new Consumer<T>(state, consumer, schema, conf);
            }
            catch(Exception e)
            {
                _actorSystem.Log.Warning($"[{namespaceName}] Failed to get topics under namespace");
                throw e;
            }
        }
        private IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
        {
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic, @"\:\/\/")[1]).Success).ToList();
        }
        private Mode? ConvertRegexSubscriptionMode(RegexSubscriptionMode regexSubscriptionMode)
        {
            switch (regexSubscriptionMode)
            {
                case RegexSubscriptionMode.PersistentOnly:
                    return Mode.Persistent;
                case RegexSubscriptionMode.NonPersistentOnly:
                    return Mode.NonPersistent;
                case RegexSubscriptionMode.AllTopics:
                    return Mode.All;
                default:
                    return null;
            }
        }
        private async ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string topic)
        {
            try
            {
                var topicName = TopicName.Get(topic);
                var result = await _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(topicName));
                if (result.Failed)
                    throw result.Exception;

                return result.ConvertTo<PartitionedTopicMetadata>();
            }
            catch (ArgumentException e)
            {
                throw new PulsarClientException.InvalidConfigurationException(e.Message);
            } 
        }

        public Producer<byte[]> NewProducer(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return NewProducer(ISchema<object>.Bytes, producerConfigBuilder);
        }
        public async ValueTask<Producer<byte[]>> NewProducerAsync(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return await NewProducerAsync(ISchema<object>.Bytes, producerConfigBuilder).ConfigureAwait(false);
        }
        public Producer<T> NewProducer<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            return NewProducerAsync(schema, configBuilder).GetAwaiter().GetResult();
        }
        public async ValueTask<Producer<T>> NewProducerAsync<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            var interceptors = configBuilder.GetInterceptors;
            configBuilder.Schema(schema);
            var conf = configBuilder.Build();
            // config validation
            Condition.CheckArgument(!(conf.BatchingEnabled && conf.ChunkingEnabled), "Batching and chunking of messages can't be enabled together");
            if (conf.TopicName == null)
            {
                throw new ArgumentException("Topic name must be set on the producer builder");
            }
            if(interceptors == null || interceptors.Count == 0)
            {
                return await CreateProducer(conf, schema).ConfigureAwait(false);
            }
            else
            {
                return await CreateProducer(conf, schema, new ProducerInterceptors<T>(_actorSystem.Log, interceptors)).ConfigureAwait(false);
            }
        }

        public Reader<byte[]> NewReader(ReaderConfigBuilder<byte[]> conf)
        {
            return NewReaderAsync(conf).GetAwaiter().GetResult();
        }
        public async ValueTask<Reader<byte[]>> NewReaderAsync(ReaderConfigBuilder<byte[]> conf)
        {
            return await NewReaderAsync(ISchema<object>.Bytes, conf).ConfigureAwait(false);
        }

        public Reader<T> NewReader<T>(ISchema<T> schema, ReaderConfigBuilder<T> confBuilder)
        {
            return NewReaderAsync(schema, confBuilder).GetAwaiter().GetResult();
        }

        public async ValueTask<Reader<T>> NewReaderAsync<T>(ISchema<T> schema, ReaderConfigBuilder<T> confBuilder)
        {
            var state = await _client.Ask<int>(GetClientState.Instance).ConfigureAwait(false);
            if (state != 0)
            {
                throw new PulsarClientException.AlreadyClosedException("Client already closed");
            }

            if (confBuilder == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined");
            }
            var conf = confBuilder.ReaderConfigurationData;

            foreach (var topic in conf.TopicNames)
            {
                if (!TopicName.IsValid(topic))
                {
                    throw new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'");
                }
            }

            if (conf.StartMessageId == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Invalid startMessageId");
            }

            if (conf.TopicNames.Count == 1)
            {
                var schemaClone = await PreProcessSchemaBeforeSubscribe(schema, conf.TopicName).ConfigureAwait(false); 
                return await CreateSingleTopicReader(conf, schemaClone).ConfigureAwait(false);
            }
            return await CreateMultiTopicReader(conf, schema).ConfigureAwait(false);
        }

        private async ValueTask<Reader<T>> CreateSingleTopicReader<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            var topic = conf.TopicName;
            try
            {
                var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                if (metadata.Partitions > 0 && MultiTopicsConsumer<T>.IsIllegalMultiTopicsMessageId(conf.StartMessageId))
                {
                    throw new PulsarClientException("The partitioned topic startMessageId is illegal");
                }
                IActorRef reader;
                var stateA = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
                if (metadata.Partitions > 0)
                {
                    reader = _actorSystem.ActorOf(Props.Create(()=> new MultiTopicsReader<T>(stateA, _client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, _clientConfigurationData)));
                    var response = await reader.Ask<AskResponse>(new Subscribe(topic, metadata.Partitions)).ConfigureAwait(false);

                    if (response.Failed)
                        throw response.Exception;

                    _client.Tell(new AddConsumer(reader));

                    return new Reader<T>(stateA, reader, schema, conf);
                }
                else
                {
                    var consumerId = await _generator.Ask<long>(NewConsumerId.Instance).ConfigureAwait(false);
                    reader = _actorSystem.ActorOf(Props.Create(()=> new ReaderActor<T>(consumerId, stateA, _client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, _clientConfigurationData)));
                    var response = await reader.Ask<AskResponse>(Connect.Instance).ConfigureAwait(false);
                    if (response.Failed)
                        throw response.Exception;

                    _client.Tell(new AddConsumer(reader));

                    return new Reader<T>(stateA, reader, schema, conf);
                }
            }
            catch(Exception ex)
            {
                _actorSystem.Log.Warning($"[{topic}] Failed to get create topic reader: {ex}");
                throw;
            }
        }
        private async ValueTask<Reader<T>> CreateMultiTopicReader<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            var stateA = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
            var reader = _actorSystem.ActorOf(Props.Create(() => new MultiTopicsReader<T>(stateA, _client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, _clientConfigurationData)));
            var response = await reader.Ask<AskResponse>(new SubscribeAndCreateTopicsIfDoesNotExist(conf.TopicNames.ToList(), true)).ConfigureAwait(false);

            if (response.Failed)
                throw response.Exception;

            _client.Tell(new AddConsumer(reader));

            return new Reader<T>(stateA, reader, schema, conf);
        }
        public TransactionBuilder NewTransaction()
        {
            if(!_clientConfigurationData.EnableTransaction)
                throw new PulsarClientException.InvalidConfigurationException("Transactions are not enabled");

            return new TransactionBuilder(_actorSystem, _client, _transactionCoordinatorClient, _actorSystem.Log);
        }

        public void Shutdown()
        {
            ShutdownAsync().ConfigureAwait(false);
        }
        public async Task ShutdownAsync()
        {
            await _actorSystem.Terminate().ConfigureAwait(false);
        }
        public ActorSystem ActorSystem => _actorSystem;
        private IActorRef ClientCnx => _client;
        public void UpdateServiceUrl(string serviceUrl)
        {
            _client.Tell(new UpdateServiceUrl(serviceUrl));
        }
        #region private matters
        private async ValueTask<Producer<byte[]>> CreateProducer(ProducerConfigurationData conf)
        {
            return await CreateProducer(conf, ISchema<object>.Bytes, null).ConfigureAwait(false);
        }
        private async ValueTask<Producer<T>> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema)
        {
            return await CreateProducer(conf, schema, null).ConfigureAwait(false);
        }

        private async ValueTask<Producer<T>> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
        {
            if (conf == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Producer configuration undefined");
            }

            if (schema is AutoConsumeSchema)
            {
                throw new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically");
            }
            
            var state = await _client.Ask<int>(GetClientState.Instance).ConfigureAwait(false);
            if (state != 0)
            {
                throw new PulsarClientException.AlreadyClosedException($"Client already closed : state = {state}");
            }

            var topic = conf.TopicName;

            if (!TopicName.IsValid(topic))
            {
                throw new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'");
            }

            if (schema is AutoProduceBytesSchema<T> autoProduceBytesSchema)
            {
                if (autoProduceBytesSchema.SchemaInitialized())
                {
                    return await CreateProducer(topic, conf, schema, interceptors).ConfigureAwait(false);
                }
                else
                {
                    var schem = await _lookup.Ask<AskResponse>(new GetSchema(TopicName.Get(conf.TopicName))).ConfigureAwait(false);
                    if (schem.Failed) 
                        throw schem.Exception;

                    var sc = schem.ConvertTo<GetSchemaInfoResponse>();
                    if (sc.SchemaInfo != null)
                    {
                        autoProduceBytesSchema.Schema = (ISchema<T>)ISchema<T>.GetSchema(sc.SchemaInfo);
                    }
                    else
                    {
                        autoProduceBytesSchema.Schema = (ISchema<T>)ISchema<T>.Bytes;
                    }
                    return await CreateProducer(topic, conf, schema, interceptors).ConfigureAwait(false);
                }
            }
            return await CreateProducer(topic, conf, schema, interceptors).ConfigureAwait(false);
        }

        private async ValueTask<Producer<T>> CreateProducer<T>(string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
        {            
            var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
            if (_actorSystem.Log.IsDebugEnabled)
            {
                _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
            }
            if (metadata.Partitions > 0)
            {
                var partitionActor = _actorSystem.ActorOf(Props.Create(()=> new PartitionedProducer<T>(_client, _lookup, _cnxPool, _generator, topic, conf, metadata.Partitions, schema, interceptors, _clientConfigurationData)));
                var co = await partitionActor.Ask<AskResponse>(Connect.Instance, TimeSpan.FromMilliseconds(_clientConfigurationData.OperationTimeoutMs));
                if (co.Failed)
                    throw co.Exception;

                _client.Tell(new AddProducer(partitionActor));
                return new Producer<T>(partitionActor, schema, conf);
            }
            else
            {
                var producerId = await _generator.Ask<long>(NewProducerId.Instance).ConfigureAwait(false);
                var producer = _actorSystem.ActorOf(Props.Create(()=> new ProducerActor<T>(producerId, _client, _lookup, _cnxPool, _generator, topic, conf, -1, schema, interceptors, _clientConfigurationData)));
                var co = await producer.Ask<AskResponse>(Connect.Instance, TimeSpan.FromMilliseconds(_clientConfigurationData.OperationTimeoutMs));
                if (co.Failed)
                    throw co.Exception;

                _client.Tell(new AddProducer(producer));

                return new Producer<T>(producer, schema, conf);
            }
            
        }
        // Check topics are valid.
        // - each topic is valid,
        // - topic names are unique.
        private bool TopicNamesValid(ICollection<string> topics)
        {
            if (topics.Count < 1)
                throw new ArgumentException("topics should contain more than 1 topic");

            Option<string> result = topics.Where(t => !TopicName.IsValid(t)).FirstOrDefault();

            if (result.HasValue)
            {
                _log.Warning($"Received invalid topic name: {result}");
                return false;
            }

            // check topic names are unique
            var set = new HashSet<string>(topics);
            if (set.Count == topics.Count)
            {
                return true;
            }
            else
            {
                _log.Warning($"Topic names not unique. unique/all : {set.Count}/{topics.Count}");
                return false;
            }
        }
        public ValueTask<IList<string>> GetPartitionsForTopicAsync(string topic)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
