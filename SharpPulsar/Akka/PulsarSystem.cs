using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarSystem: IAsyncDisposable
    {
        private ActorSystem _actorSystem;
        private IActorRef _pulsarManager;
        private ClientConfigurationData _conf;

        public PulsarSystem(ClientConfigurationData conf)
        {
            _conf = conf;
            var config = ConfigurationFactory.ParseString(@"
            akka
            {
                loglevel = DEBUG
			    log-config-on-start = on 
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
            }"
            );
            _actorSystem = ActorSystem.Create("Pulsar", config);
            _pulsarManager = _actorSystem.ActorOf(PulsarManager.Prop(conf), "PulsarManager");
        }

        /// <summary>
        /// Create a producer Actor. Topic is returned for uniformity purposes!
        /// </summary>
        /// <param name="producer"></param>
        /// <returns>string</returns>
        public string CreateProducer(CreateProducer producer)
        {
            var conf = producer.ProducerConfiguration;

            if (!TopicName.IsValid(conf.TopicName))
                throw new ArgumentException("Topic is invalid");

            var topic = TopicName.Get(conf.TopicName);
            conf.TopicName = topic.ToString();
            var p = new NewProducer(producer.Schema, _conf, conf);
            _pulsarManager.Tell(p);
            return topic.ToString();
        }
        public void CreateReader(CreateReader reader)
        {
            if (reader.Seek != null && (reader.Seek.Type == null || reader.Seek.Input == null))
                throw new ArgumentException("Seek is in an invalid state: null Type or Input");
            var p = new NewReader(reader.Schema, _conf, reader.ReaderConfiguration, reader.Seek);
            _pulsarManager.Tell(p);
        }

        public void QueryData(QueryData data)
        {
            if(string.IsNullOrWhiteSpace(data.DestinationServer) || data.ExceptionHandler == null || data.Handler == null || string.IsNullOrWhiteSpace(data.Query))
                throw new ArgumentException("QueryData is in an invalid state: null field not allowed");
            _pulsarManager.Tell(data);
        }
        public void SetupSqlServers(SqlServers servers)
        {
            if (servers.Servers.Count < 1)
                throw new ArgumentException("SqlServers is in an invalid state: Servers must be greater than 1");
            _pulsarManager.Tell(servers);
        }
        public void CreateConsumer(CreateConsumer consumer)
        {
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
            if(consumer.Seek != null && (consumer.Seek.Type == null || consumer.Seek.Input == null))
                throw new ArgumentException("Seek is in an invalid state: null Type or Input");
            var c = new NewConsumer(consumer.Schema, _conf, consumer.ConsumerConfiguration, consumer.ConsumerType, consumer.Seek);
            _pulsarManager.Tell(c);
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
