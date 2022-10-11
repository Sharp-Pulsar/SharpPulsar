using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Util;
using SharpPulsar.Builder;
using SharpPulsar.Cache;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Table;
using SharpPulsar.Utils;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
namespace SharpPulsar
{
    public class PulsarClient : IPulsarClient
    {
        private readonly IActorRef _client;
        private readonly IActorRef _transactionCoordinatorClient;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly ActorSystem _actorSystem;
        private readonly Cache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = new Cache<string, ISchemaInfoProvider>(TimeSpan.FromMinutes(30), 100000);
        private readonly ILoggingAdapter _log;
        private readonly IActorRef _cnxPool;
        private IActorRef _lookup;
        private readonly IActorRef _generator;
        private Backoff _backoff;
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
            _lookup = _actorSystem.ActorOf(BinaryProtoLookupService.Prop(_cnxPool, _generator, _clientConfigurationData.ServiceUrl, _clientConfigurationData.ListenerName, _clientConfigurationData.UseTls, _clientConfigurationData.MaxLookupRequest, _clientConfigurationData.OperationTimeout, _clientConfigurationData.ClientCnx), "BinaryProtoLookupService");
            _lookup.Tell(new SetClient(_client));
        }

        public IList<string> GetPartitionsForTopic(string topic)
        {
            throw new NotImplementedException();
        }
        private ISchemaInfoProvider NewSchemaProvider(string topicName)
        {
            return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), _log, _lookup);
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
                    {
                        _schemaProviderLoadingCache.Put(topicName, NewSchemaProvider(topicName));
                        schemaInfoProvider = _schemaProviderLoadingCache.Get(topicName);
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Failed to load schema info provider for topic {topicName}: {e}");
                    throw;
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

            if (conf.KeySharedPolicy != null && conf.SubscriptionType != SubType.KeyShared)
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

            if (conf.ReadCompacted && (conf.TopicNames.Any(topic => TopicName.Get(topic).Domain != TopicDomain.Persistent) || conf.SubscriptionType != SubType.Exclusive && conf.SubscriptionType != SubType.Failover))
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
            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            IActorRef cnsr = Nobody.Instance;
            try
            {
                var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                var state = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
                if (metadata.Partitions > 0)
                {
                    Condition.CheckArgument(conf.TopicNames.Count == 1, "Should have only 1 topic for partitioned consumer");

                    // get topic name, then remove it from conf, so constructor will create a consumer with no topic.
                    var cloneConf = conf;
                    var topicName = cloneConf.SingleTopic;
                    cloneConf.TopicNames.Remove(topicName);
                    var consumer = _actorSystem.ActorOf(MultiTopicsConsumer<T>.Prop(state, _client, _lookup, _cnxPool, _generator, topicName, conf, schema, true, _clientConfigurationData, tcs), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
                    cnsr = await tcs.Task.ConfigureAwait(false);

                    _client.Tell(new AddConsumer(cnsr));
                    return new Consumer<T>(state, cnsr, schema, conf, _clientConfigurationData.OperationTimeout);
                }
                else
                {
                    var consumerId = await _generator.Ask<long>(NewConsumerId.Instance).ConfigureAwait(false);
                    var partitionIndex = TopicName.GetPartitionIndex(topic);
                    var consumer = _actorSystem.ActorOf(ConsumerActor<T>.Prop(consumerId, state, _client, _lookup, _cnxPool, _generator, topic, conf, partitionIndex, false, null, schema, true, _clientConfigurationData, tcs));
                    cnsr = await tcs.Task.ConfigureAwait(false);

                    _client.Tell(new AddConsumer(cnsr));
                    return new Consumer<T>(state, cnsr, schema, conf, _clientConfigurationData.OperationTimeout);
                }
            }
            catch (Exception e)
            {
                _log.Error($"[{topic}] Failed to get partitioned topic metadata: {e}");
                await cnsr.GracefulStop(TimeSpan.FromSeconds(1));
                throw;
            }
        }

        private async ValueTask<Consumer<T>> MultiTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            Condition.CheckArgument(conf.TopicNames.Count == 0 || TopicNamesValid(conf.TopicNames), "Topics is empty or invalid.");

            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            var state = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
            var consumer = _actorSystem.ActorOf(MultiTopicsConsumer<T>.Prop(state, _client, _lookup, _cnxPool, _generator, conf, schema, conf.ForceTopicCreation, _clientConfigurationData, tcs), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
            var cnsr = await tcs.Task.ConfigureAwait(false);

            _client.Tell(new AddConsumer(cnsr));
            return new Consumer<T>(state, cnsr, schema, conf, _clientConfigurationData.OperationTimeout);
        }

        private async ValueTask<Consumer<T>> PatternTopicSubscribe<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            var regex = conf.TopicsPattern.ToString();
            var subscriptionMode = ConvertRegexSubscriptionMode(conf.RegexSubscriptionMode);
            var destination = TopicName.Get(regex);
            var namespaceName = destination.NamespaceObject;
            IActorRef consumer = null;
            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            var ask = await _lookup.Ask<AskResponse>(new GetTopicsUnderNamespace(namespaceName, subscriptionMode.Value, regex, null)).ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;

            var result = ask.ConvertTo<GetTopicsUnderNamespaceResponse>();
            var topicsList = result.Topics;
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Get topics under namespace {namespaceName}, topics.size: {topicsList.Count}, topicsHash: {result.TopicsHash}, changed: {result.Changed}, filtered: {result.Topics}");
                topicsList.ForEach(topicName => _log.Debug($"Get topics under namespace {namespaceName}, topic: {topicName}"));
            }
            if (!result.Filtered)
            {
                topicsList = TopicList.FilterTopics(result.Topics, conf.TopicsPattern).ToImmutableList();
            }

            topicsList.ToList().ForEach(x => conf.TopicNames.Add(x));
            var state = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");

            consumer = _actorSystem.ActorOf(PatternMultiTopicsConsumer<T>.Prop(conf.TopicsPattern, result.TopicsHash, state, _client, _lookup, _cnxPool, _generator, conf, schema, subscriptionMode.Value, _clientConfigurationData, tcs), $"MultiTopicsConsumer{DateTimeHelper.CurrentUnixTimeMillis()}");
            var cnsr = await tcs.Task.ConfigureAwait(false);

            _client.Tell(new AddConsumer(cnsr));
            return new Consumer<T>(state, cnsr, schema, conf, _clientConfigurationData.OperationTimeout);
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
            var future = new TaskCompletionSource<PartitionedTopicMetadata>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                var topicName = TopicName.Get(topic);
                var opTimeoutMs = Conf.LookupTimeoutMs;
                var backoff = new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(Conf.InitialBackoffIntervalMs))
                    .SetMandatoryStop(TimeSpan.FromMilliseconds(opTimeoutMs * 2))
                    .SetMax(TimeSpan.FromMilliseconds(Conf.InitialBackoffIntervalMs)).Create();
                await GetPartitionedTopicMetadata(topicName, backoff, opTimeoutMs, future, new List<Exception>());

            }
            catch (ArgumentException e)
            {
                future.SetException(new PulsarClientException.InvalidConfigurationException(e.Message));
            }
            return future.Task.GetAwaiter().GetResult();
        }
        private async ValueTask GetPartitionedTopicMetadata(TopicName topicName, Backoff backoff, long remainingTime, TaskCompletionSource<PartitionedTopicMetadata> future, IList<Exception> previousExceptions)
        {

            var startTime = NanoTime();
            var result = await _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(topicName));
            if (result.Failed)
            {
                var remaining = -1 * TimeSpan.FromMilliseconds(NanoTime() + startTime);
                var nextDelay = Math.Min(backoff.Next(), remainingTime);
                var isLookupThrottling = !PulsarClientException.IsRetriableError(result.Exception) || result.Exception is PulsarClientException.AuthenticationException;
                if (nextDelay <= 0 || isLookupThrottling)
                {
                    //PulsarClientException.PreviousExceptions(previousExceptions.Add(result.Exception));
                    future.SetException(result.Exception);
                    return;
                }
                _log.Warning($"[topic: {topicName}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {nextDelay} ms: {result.Exception?.Message}");

                previousExceptions.Add(result.Exception);
                var time = (long)(remaining - TimeSpan.FromMilliseconds(nextDelay)).TotalMilliseconds;
                await GetPartitionedTopicMetadata(topicName, backoff, time, future, previousExceptions);
                return;
            }

            future.SetResult(result.ConvertTo<PartitionedTopicMetadata>());
        }
        private static long NanoTime()
        {
            var nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
        /// <summary>
        /// Create a producer builder that can be used to configure
        /// and construct a producer with default <seealso cref="ISchema{}.BYTES"/>.
        /// 
        /// <para>Example:
        /// 
        /// <pre>{@code
        /// Producer<byte[]> producer = client.newProducer()
        ///                  .topic("my-topic")
        ///                  .create();
        /// producer.send("test".getBytes());
        /// }</pre> 
        /// </para>
        /// </summary> 
        /// <param name="producerConfigBuilder"></param>
        /// <returns> <seealso cref="Producer{byte[]}"/> instance @since 2.0.0 </returns>

        public Producer<byte[]> NewProducer(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return NewProducer(ISchema<object>.Bytes, producerConfigBuilder);
        }
        public PartitionedProducer<byte[]> NewPartitionedProducer(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return NewPartitionedProducer(ISchema<object>.Bytes, producerConfigBuilder);
        }
        /// <inheritdoc cref="NewProducer(ProducerConfigBuilder{byte[]})"/>
        public async ValueTask<Producer<byte[]>> NewProducerAsync(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return await NewProducerAsync(ISchema<object>.Bytes, producerConfigBuilder).ConfigureAwait(false);
        }
        public async ValueTask<PartitionedProducer<byte[]>> NewPartitionedProducerAsync(ProducerConfigBuilder<byte[]> producerConfigBuilder)
        {
            return await NewPartitionedProducerAsync(ISchema<object>.Bytes, producerConfigBuilder).ConfigureAwait(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schema"></param>
        /// <param name="configBuilder"></param>
        /// <returns></returns>
        public Producer<T> NewProducer<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            return NewProducerAsync(schema, configBuilder).GetAwaiter().GetResult();
        }
        public PartitionedProducer<T> NewPartitionedProducer<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            return NewPartitionedProducerAsync(schema, configBuilder).GetAwaiter().GetResult();
        }
        public async ValueTask<Producer<T>> NewProducerAsync<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            var producer = (Producer<T>)await ProducerAsync(schema, configBuilder);
            return producer;
        }
        public async ValueTask<PartitionedProducer<T>> NewPartitionedProducerAsync<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
        {
            var partitionedProducer = (PartitionedProducer<T>)await ProducerAsync(schema, configBuilder);
            return partitionedProducer;
        }
        private async ValueTask<object> ProducerAsync<T>(ISchema<T> schema, ProducerConfigBuilder<T> configBuilder)
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
            if (interceptors == null || interceptors.Count == 0)
            {
                return await CreateProducer(conf, schema).ConfigureAwait(false);
            }
            else
            {
                return await CreateProducer(conf, schema, new ProducerInterceptors<T>(_log, interceptors)).ConfigureAwait(false);
            }
        }
        public Reader<byte[]> NewReader(ReaderConfigBuilder<byte[]> conf)
        {
            return NewReaderAsync(conf).GetAwaiter().GetResult();
        }
        public Reader<T> NewReader<T>(ISchema<T> schema, ReaderConfigBuilder<T> confBuilder)
        {
            return NewReaderAsync(schema, confBuilder).GetAwaiter().GetResult();
        }
        public async ValueTask<Reader<byte[]>> NewReaderAsync(ReaderConfigBuilder<byte[]> conf)
        {
            return await NewReaderAsync(ISchema<object>.Bytes, conf).ConfigureAwait(false);
        }

        public ITableViewBuilder<T> NewTableViewBuilder<T>(ISchema<T> Schema)
        {
            return new TableViewBuilder<T>(this, Schema);
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
            IActorRef actorRef = Nobody.Instance;
            try
            {
                var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
                }
                if (metadata.Partitions > 0 && MultiTopicsConsumer<T>.IsIllegalMultiTopicsMessageId(conf.StartMessageId))
                {
                    throw new PulsarClientException("The partitioned topic startMessageId is illegal");
                }
                var stateA = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
                if (metadata.Partitions > 0)
                {
                    _actorSystem.ActorOf(MultiTopicsReader<T>.Prop(stateA, _client, _lookup, _cnxPool, _generator, conf, schema, _clientConfigurationData, tcs));
                    actorRef = await tcs.Task.ConfigureAwait(false);

                    _client.Tell(new AddConsumer(actorRef));
                    return new Reader<T>(stateA, actorRef, schema, conf);
                }
                else
                {
                    var consumerId = await _generator.Ask<long>(NewConsumerId.Instance).ConfigureAwait(false);
                    _actorSystem.ActorOf(Props.Create(() => new ReaderActor<T>(consumerId, stateA, _client, _lookup, _cnxPool, _generator, conf, schema, _clientConfigurationData, tcs)));

                    actorRef = await tcs.Task.ConfigureAwait(false);

                    _client.Tell(new AddConsumer(actorRef));
                    return new Reader<T>(stateA, actorRef, schema, conf);
                }
            }
            catch (Exception ex)
            {
                await actorRef.GracefulStop(TimeSpan.FromSeconds(1));
                _log.Warning($"[{topic}] Failed to get create topic reader: {ex}");
                throw;
            }
        }
        private async ValueTask<Reader<T>> CreateMultiTopicReader<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            var stateA = _actorSystem.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
            _actorSystem.ActorOf(Props.Create(() => new MultiTopicsReader<T>(stateA, _client, _lookup, _cnxPool, _generator, conf, schema, _clientConfigurationData, tcs)));
            var cnsr = await tcs.Task.ConfigureAwait(false);

            _client.Tell(new AddConsumer(cnsr));
            return new Reader<T>(stateA, cnsr, schema, conf);
        }
        public TransactionBuilder NewTransaction()
        {
            if (!_clientConfigurationData.EnableTransaction)
                throw new PulsarClientException.InvalidConfigurationException("Transactions are not enabled");

            return new TransactionBuilder(_actorSystem, _client, _transactionCoordinatorClient, _log);
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
        public ILoggingAdapter Log => _log;

        public void UpdateServiceUrl(string serviceUrl)
        {
            _client.Tell(new UpdateServiceUrl(serviceUrl));
        }
        public void UpdateAuthentication(IAuthentication authentication)
        {
            _log.Info($"Updating authentication to {authentication}");
            if (_clientConfigurationData.Authentication != null)
            {
                _clientConfigurationData.Authentication.Dispose();
            }
            _clientConfigurationData.Authentication = authentication;
            _clientConfigurationData.Authentication.Start();
        }

        public void UpdateTlsTrustCertsFilePath(string tlsTrustCertsFilePath)
        {
            _log.Info($"Updating tlsTrustCertsFilePath to {tlsTrustCertsFilePath}");
            _clientConfigurationData.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
        }

        public virtual void UpdateTlsTrustStorePathAndPassword(string tlsTrustStorePath, string tlsTrustStorePassword)
        {
            _log.Info($"Updating tlsTrustStorePath to {tlsTrustStorePath}, tlsTrustStorePassword to *****");
            _clientConfigurationData.TlsTrustStorePath = tlsTrustStorePath;
            _clientConfigurationData.TlsTrustStorePassword = tlsTrustStorePassword;
        }
        public ClientConfigurationData Conf => _clientConfigurationData;
        #region private matters
        private async ValueTask<object> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema)
        {
            return await CreateProducer(conf, schema, null).ConfigureAwait(false);
        }

        private async ValueTask<object> CreateProducer<T>(ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
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

        private async ValueTask<object> CreateProducer<T>(string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors)
        {
            var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
            }
            if (metadata.Partitions > 0)
            {
                var tcs = new TaskCompletionSource<ConcurrentDictionary<int, IActorRef>>(TaskCreationOptions.RunContinuationsAsynchronously);

                var partitionActor = _actorSystem.ActorOf(PartitionedProducerActor<T>.Prop(_client, _lookup, _cnxPool, _generator, topic, conf, metadata.Partitions, schema, interceptors, _clientConfigurationData, null, tcs));

                try
                {

                    var con = await tcs.Task.ConfigureAwait(false);
                    //var producer = partitionActor;// await tcs.Task;

                    _client.Tell(new AddProducer(partitionActor));
                    return new PartitionedProducer<T>(partitionActor, schema, conf, _clientConfigurationData.OperationTimeout, con);
                }
                catch
                {
                    await partitionActor.GracefulStop(TimeSpan.FromSeconds(5));
                    throw;
                }
            }
            else
            {
                var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                var producerId = await _generator.Ask<long>(NewProducerId.Instance).ConfigureAwait(false);
                var producer = _actorSystem.ActorOf(ProducerActor<T>.Prop(producerId, _client, _lookup, _cnxPool, _generator, topic, conf, tcs, -1, schema, interceptors, _clientConfigurationData, null));
                try
                {
                    _ = await tcs.Task.ConfigureAwait(false);

                    _client.Tell(new AddProducer(producer));

                    return new Producer<T>(producer, schema, conf, _clientConfigurationData.OperationTimeout);
                }
                catch
                {
                    await producer.GracefulStop(TimeSpan.FromSeconds(5));
                    throw;
                }
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
        public async ValueTask<IList<string>> GetPartitionsForTopicAsync(string topic)
        {
            var metadata = await GetPartitionedTopicMetadata(topic).ConfigureAwait(false);
            if (metadata.Partitions > 0)
            {
                var topicName = TopicName.Get(topic);
                var partitions = new List<string>(metadata.Partitions);
                for (var i = 0; i < metadata.Partitions; i++)
                {
                    partitions.Add(topicName.GetPartition(i).ToString());
                }
                return partitions;
            }
            else
            {
                return new List<string> { topic };
            }
        }
        #endregion
    }
}
