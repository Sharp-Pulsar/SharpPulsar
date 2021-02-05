using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Model;
using SharpPulsar.Precondition;
using SharpPulsar.Queues;
using SharpPulsar.Schema;
using SharpPulsar.Schemas;
using SharpPulsar.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.User
{
    public class PulsarClient : IPulsarClient
    {
        private readonly IActorRef _client;
        private readonly IActorRef _transactionCoordinatorClient;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly ActorSystem _actorSystem;

        public PulsarClient(IActorRef client, ClientConfigurationData clientConfiguration, ActorSystem actorSystem, IActorRef transactionCoordinatorClient)
        {
            _client = client;
            _clientConfigurationData = clientConfiguration;
            _actorSystem = actorSystem;
            _transactionCoordinatorClient = transactionCoordinatorClient;
        }
        public bool Closed => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IList<string> GetPartitionsForTopic(string topic)
        {
            throw new NotImplementedException();
        }

        public Consumer<sbyte[]> NewConsumer(ConsumerConfigurationData<sbyte[]> conf)
        {
            //create ConsumerActor first
            throw new NotImplementedException();
        }

        public Consumer<T> NewConsumer<T>(ISchema<T> schema, ConsumerConfigurationData<T> conf)
        {
            throw new NotImplementedException();
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

        public Reader<sbyte[]> NewReader(ReaderConfigurationData<sbyte[]> conf)
        {
            throw new NotImplementedException();
        }

        public Reader<T> NewReader<T>(ISchema<T> schema, ReaderConfigurationData<T> conf)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
                            autoProduceBytesSchema.Schema = (ISchema<T>)(object)ISchema<T>.GetSchema(sc.SchemaInfo);
                        }
                        else
                        {
                            autoProduceBytesSchema.Schema = (ISchema<T>)(object)ISchema<T>.Bytes;
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
            var metadata = _client.AskFor<LookupDataResult>(new GetPartitionedTopicMetadata(TopicName.Get(topic)));
            if (_actorSystem.Log.IsDebugEnabled)
            {
                _actorSystem.Log.Debug($"[{topic}] Received topic metadata. partitions: {metadata.Partitions}");
            }
            IActorRef producer;
            if (metadata.Partitions > 0)
            {
                producer = _actorSystem.ActorOf(PartitionedProducer<T>.Prop(_client, topic, conf, metadata.Partitions, schema, interceptors, _clientConfigurationData, queue));
            }
            else
            {
                producer = _actorSystem.ActorOf(ProducerActor<T>.Prop(_client, topic, conf, -1, schema, interceptors, _clientConfigurationData, queue));
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
