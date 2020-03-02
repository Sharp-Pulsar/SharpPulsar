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
        
        
        public void CreateProducer(CreateProducer producer)
        {
            var p = new NewProducer(producer.Schema, _conf, producer.ProducerConfiguration);
            _pulsarManager.Tell(p);
        }
        public void CreateReader(CreateReader reader)
        {
            var p = new NewReader(reader.Schema, _conf, reader.ReaderConfiguration);
            _pulsarManager.Tell(p);
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
            var c = new NewConsumer(consumer.Schema, _conf, consumer.ConsumerConfiguration, consumer.ConsumerType);
            _pulsarManager.Tell(c);
        }

        public void Send(Send send, IActorRef producer)
        {
           producer.Tell(send);
        }
        public void BatchSend(BulkSend send, IActorRef producer)
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
            var result = topics.Where(topic =>
            {
                var topicInvalid = !TopicName.IsValid(topic);
                if (topicInvalid)
                {
                    return true;
                }
                var newNamespace = TopicName.Get(topic).Namespace;
                return !@namespace.Equals(newNamespace);
            }).First();

            if (!string.IsNullOrWhiteSpace(result))
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
