using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Cache;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Model;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Queues;
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


        private ISchema<T> PreProcessSchemaBeforeSubscribe<T>(ISchema<T> schema, string topicName)
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
                    var schemaInfo = schemaInfoProvider.LatestSchema;
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
        public Consumer<sbyte[]> NewConsumer(ConsumerConfigBuilder<sbyte[]> conf)
        {
            return NewConsumer(ISchema<sbyte[]>.Bytes, conf);
        }

        public Consumer<T> NewConsumer<T>(ISchema<T> schema, ConsumerConfigBuilder<T> confBuilder)
        {            
            var conf = confBuilder.ConsumerConfigurationData;
            // DLQ only supports non-ordered subscriptions, don't enable DLQ on Key_Shared subType since it require message ordering for given key.
            if (conf.SubscriptionType == SubType.KeyShared && !string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
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
                TopicName topicFirst = TopicName.Get(conf.TopicNames.GetEnumerator().Current);
                string retryLetterTopic = topicFirst.Namespace + "/" + conf.SubscriptionName + RetryMessageUtil.RetryGroupTopicSuffix;
                string deadLetterTopic = topicFirst.Namespace + "/" + conf.SubscriptionName + RetryMessageUtil.DlqGroupTopicSuffix;
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
            var interceptors = conf.Interceptors;
            if (interceptors == null || interceptors.Count == 0)
            {
                return Subscribe(conf, schema, null);
            }
            else
            {
                return Subscribe(conf, schema, new ConsumerInterceptors<T>(_actorSystem, interceptors));
            }
        }
        
        private Consumer<T> Subscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
        {
            var state = _client.AskFor<int>(GetClientState.Instance);
            if (state != 0)
            {
                throw new PulsarClientException.AlreadyClosedException("Client already closed");
            }

            if (conf == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined");
            }

            foreach (string topic in conf.TopicNames)
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

            if (conf.ReadCompacted && (!conf.TopicNames.All(topic => TopicName.Get(topic).Domain == TopicDomain.Persistent) || (conf.SubscriptionType != SubType.Exclusive && conf.SubscriptionType != SubType.Failover)))
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
                if (conf.TopicNames.Count == 0)
                {
                    throw new ArgumentException("Topic names list must be null when use topicsPattern");
                }
                return PatternTopicSubscribe(conf, schema, interceptors);
            }
            else if (conf.TopicNames.Count == 1)
            {
                return SingleTopicSubscribe(conf, schema, interceptors);
            }
            else
            {
                return MultiTopicSubscribe(conf, schema, interceptors);
            }
        }

        private Consumer<T> SingleTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
        {
            var schemaClone = PreProcessSchemaBeforeSubscribe(schema, conf.SingleTopic);
            return DoSingleTopicSubscribe(conf, schemaClone, interceptors);
        }

        private Consumer<T> DoSingleTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
        {
            var queue = new ConsumerQueueCollections<T>();
            string topic = conf.SingleTopic;
            try
            {
                var metadata = GetPartitionedTopicMetadata(topic);
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                IActorRef consumer;
                if (metadata.Partitions > 0)
                {
                    consumer = _actorSystem.ActorOf(MultiTopicsConsumer<T>.CreatePartitionedConsumer(_client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, interceptors, _clientConfigurationData, queue));
                    consumer.Tell(new Subscribe(topic, metadata.Partitions));
                }
                else
                {
                    int partitionIndex = TopicName.GetPartitionIndex(topic);
                    consumer = _actorSystem.ActorOf(ConsumerActor<T>.NewConsumer(_client, _lookup, _cnxPool, _generator, topic, conf, _actorSystem.Scheduler.Advanced, partitionIndex, false, null, schema, interceptors, true, _clientConfigurationData, queue));
                }
                _client.Tell(new AddConsumer(consumer));
                var c = queue.ConsumerCreation.Take();
                if (c != null)
                    throw c.Exception;

                return new Consumer<T>(consumer, queue, schema, conf, interceptors);
            }
            catch(Exception e)
            {
                _actorSystem.Log.Error($"[{topic}] Failed to get partitioned topic metadata: {e}");
                throw;
            }
        }

        private Consumer<T> MultiTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
        {
            var queue = new ConsumerQueueCollections<T>();
            var consumer = _actorSystem.ActorOf(MultiTopicsConsumer<T>.NewMultiTopicsConsumer(_client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, true, schema, interceptors, _clientConfigurationData, queue));
            
            _client.Tell(new AddConsumer(consumer));
            return new Consumer<T>(consumer, queue, schema, conf, interceptors);
        }

        private Consumer<T> PatternTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
        {

            var queue = new ConsumerQueueCollections<T>();
            string regex = conf.TopicsPattern.ToString();
            var subscriptionMode = ConvertRegexSubscriptionMode(conf.RegexSubscriptionMode);
            TopicName destination = TopicName.Get(regex);
            NamespaceName namespaceName = destination.NamespaceObject;
            try
            {
               var topics = _client.AskFor<GetTopicsOfNamespaceResponse>(new GetTopicsUnderNamespace(namespaceName, subscriptionMode.Value)).Response.Topics;
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"Get topics under namespace {namespaceName}, topics.size: {topics.Count}");
                    topics.ForEach(topicName => _actorSystem.Log.Debug($"Get topics under namespace {namespaceName}, topic: {topicName}"));
                }
                IList<string> topicsList = TopicsPatternFilter(topics, conf.TopicsPattern);
                topicsList.ToList().ForEach(x => conf.TopicNames.Add(x));
                var consumer = _actorSystem.ActorOf(PatternMultiTopicsConsumer<T>.Prop(conf.TopicsPattern, _client, _lookup, _cnxPool, _generator, conf, schema, subscriptionMode.Value, interceptors, _clientConfigurationData, queue));

                _client.Tell(new AddConsumer(consumer));
                return new Consumer<T>(consumer, queue, schema, conf, interceptors);
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
        private PartitionedTopicMetadata GetPartitionedTopicMetadata(string topic)
        {

            var metadataFuture = new TaskCompletionSource<PartitionedTopicMetadata>();

            try
            {
                TopicName topicName = TopicName.Get(topic);
                var opTimeoutMs = _clientConfigurationData.OperationTimeoutMs;
                Backoff backoff = (new BackoffBuilder()).SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
                GetPartitionedTopicMetadata(topicName, backoff, opTimeoutMs, metadataFuture);
            }
            catch (ArgumentException e)
            {
                throw new PulsarClientException.InvalidConfigurationException(e.Message);
            }
            return Task.Run(() => metadataFuture.Task).Result; 
        }

        private void GetPartitionedTopicMetadata(TopicName topicName, Backoff backoff, long remainingTime, TaskCompletionSource<PartitionedTopicMetadata> future)
        {
            try
            {
                var o = _client.AskFor<PartitionedTopicMetadata>(new GetPartitionedTopicMetadata(topicName));
                future.SetResult(o);
            }
            catch(Exception e)
            {
                long nextDelay = Math.Min(backoff.Next(), remainingTime);
                bool isLookupThrottling = !PulsarClientException.IsRetriableError(e) || e is PulsarClientException.TooManyRequestsException || e is PulsarClientException.AuthenticationException;
                if (nextDelay <= 0 || isLookupThrottling)
                {
                    future.SetException(e);
                }
                Task.Run(async()=> 
                {
                    _actorSystem.Log.Warning($"[topic: {topicName}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {nextDelay} ms");
                    remainingTime -= nextDelay;
                    await Task.Delay(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(nextDelay)));
                    GetPartitionedTopicMetadata(topicName, backoff, remainingTime, future);
                });
            }
        }
        public Producer<sbyte[]> NewProducer(ProducerConfigBuilder<sbyte[]> producerConfigBuilder)
        {
            var conf = producerConfigBuilder.Build();
            return CreateProducer(conf);
        }

        public Producer<T> NewProducer<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
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
                return CreateProducer(conf, schema);
            }
            else
            {
                return CreateProducer(conf, schema, new ProducerInterceptors<T>(_actorSystem.Log, interceptors));
            }
        }

        public Reader<sbyte[]> NewReader(ReaderConfigBuilder<sbyte[]> conf)
        {
            return NewReader(ISchema<object>.Bytes, conf);
        }

        public Reader<T> NewReader<T>(ISchema<T> schema, ReaderConfigBuilder<T> confBuilder)
        {
            var conf = confBuilder.ReaderConfigurationData;
            if (conf.TopicName == null)
            {
                throw new ArgumentException("Topic name must be set on the reader builder");
            }

            if (conf.StartMessageId != null && conf.StartMessageFromRollbackDurationInSec > 0 || conf.StartMessageId == null && conf.StartMessageFromRollbackDurationInSec <= 0)
            {
                throw new ArgumentException("Start message id or start message from roll back must be specified but they cannot be specified at the same time");
            }

            if (conf.StartMessageFromRollbackDurationInSec > 0)
            {
                conf.StartMessageId = IMessageId.Earliest;
            }

            return CreateReader(conf, schema);
        }
        private Reader<T> CreateReader<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            var schemaClone = PreProcessSchemaBeforeSubscribe(schema, conf.TopicName);
            return DoCreateReader(conf, schemaClone);
        }

        private Reader<T> DoCreateReader<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            var state = _client.AskFor<int>(GetClientState.Instance);
            if (state != 0)
            { 
                throw new PulsarClientException.AlreadyClosedException("Client already closed");
            }

            if (conf == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined");
            }

            string topic = conf.TopicName;

            if (!TopicName.IsValid(topic))
            {
                throw new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'");
            }

            if (conf.StartMessageId == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Invalid startMessageId");
            }

            try
            {
                var queue = new ConsumerQueueCollections<T>();
                var metadata = GetPartitionedTopicMetadata(topic);
                if (_actorSystem.Log.IsDebugEnabled)
                {
                    _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                if (metadata.Partitions > 0 && MultiTopicsConsumer<T>.IsIllegalMultiTopicsMessageId(conf.StartMessageId))
                {
                    throw new PulsarClientException("The partitioned topic startMessageId is illegal");
                }
                IActorRef reader;
                if (metadata.Partitions > 0)
                {
                    reader = _actorSystem.ActorOf(MultiTopicsReader<T>.Prop(_client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, _clientConfigurationData, queue));                    
                }
                else
                {
                    reader = _actorSystem.ActorOf(ReaderActor<T>.Prop(_client, _lookup, _cnxPool, _generator, conf, _actorSystem.Scheduler.Advanced, schema, _clientConfigurationData, queue));
                }
                _client.Tell(new AddConsumer(reader));

                var c = queue.ConsumerCreation.Take();
                if (c != null)
                    throw c.Exception;

                return new Reader<T>(reader, queue, schema, conf);
            }
            catch(Exception ex)
            {
                _actorSystem.Log.Warning($"[{topic}] Failed to get create topic reader: {ex}");
                throw ex;
            }
        }
        public TransactionBuilder NewTransaction()
        {
            return new TransactionBuilder(_actorSystem, _client, _transactionCoordinatorClient, _actorSystem.Log);
        }

        public void Shutdown()
        {
            _actorSystem.Terminate();
        }

        public void UpdateServiceUrl(string serviceUrl)
        {
            _client.Tell(new UpdateServiceUrl(serviceUrl));
        }
        #region private matters
        private Producer<sbyte[]> CreateProducer(ProducerConfigurationData conf)
        {
            return CreateProducer(conf, ISchema<object>.Bytes, null);
        }
        private Producer<T> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema)
        {
            return CreateProducer(conf, schema, null);
        }

        private Producer<T> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
        {
            if (conf == null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Producer configuration undefined");
            }

            if (schema is AutoConsumeSchema)
            {
                throw new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically");
            }
            
            var state = _client.AskFor<int>(GetClientState.Instance);
            if (state != 0)
            {
                throw new PulsarClientException.AlreadyClosedException($"Client already closed : state = {state}");
            }

            string topic = conf.TopicName;

            if (!TopicName.IsValid(topic))
            {
                throw new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'");
            }

            if (schema is AutoProduceBytesSchema<T> autoProduceBytesSchema)
            {
                if (autoProduceBytesSchema.SchemaInitialized())
                {
                    return CreateProducer(topic, conf, schema, interceptors);
                }
                else
                {
                    var schem = _client.AskFor(new GetSchema(TopicName.Get(conf.TopicName)));
                    if (schem is Failure fa)
                    {
                        throw fa.Exception;
                    }
                    else if (schem is GetSchemaInfoResponse sc)
                    {
                        if (sc.SchemaInfo != null)
                        {
                            autoProduceBytesSchema.Schema = (ISchema<T>)ISchema<T>.GetSchema(sc.SchemaInfo);
                        }
                        else
                        {
                            autoProduceBytesSchema.Schema = (ISchema<T>)ISchema<T>.Bytes;
                        }
                    }
                    return CreateProducer(topic, conf, schema, interceptors);
                }
            }
            else
            {
                return CreateProducer(topic, conf, schema, interceptors);
            }

        }

        private Producer<T> CreateProducer<T>(string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
        {
            var queue = new ProducerQueueCollection<T>();
            var metadata = GetPartitionedTopicMetadata(topic);
            if (_actorSystem.Log.IsDebugEnabled)
            {
                _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
            }
            IActorRef producer;
            if (metadata.Partitions > 0)
            {
                producer = _actorSystem.ActorOf(PartitionedProducer<T>.Prop(_client, _generator, topic, conf, metadata.Partitions, schema, interceptors, _clientConfigurationData, queue));
            }
            else
            {
                producer = _actorSystem.ActorOf(ProducerActor<T>.Prop(_client, _generator, topic, conf, -1, schema, interceptors, _clientConfigurationData, queue));
            }
            _client.Tell(new AddProducer(producer));
            //Improve with trytake for partitioned topic too
            var created = queue.Producer.Take();
            if (created.Errored)
                throw created.Exception;

            return new Producer<T>(producer, queue, schema, conf);
        }
        #endregion
    }
}
