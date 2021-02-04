using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.User
{
    public class PulsarClient : IPulsarClient
    {
        private readonly IActorRef _client;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly ActorSystem _actorSystem;

        public PulsarClient(IActorRef client, ClientConfigurationData clientConfiguration, ActorSystem actorSystem)
        {
            _client = client;
            _clientConfigurationData = clientConfiguration;
            _actorSystem = actorSystem;
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

        public Producer<sbyte[]> NewProducer(ProducerConfigurationData conf)
        {
            throw new NotImplementedException();
        }

        public Producer<T> NewProducer<T>(ISchema<T> schema, ProducerConfigurationData conf)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void UpdateServiceUrl(string serviceUrl)
        {
            throw new NotImplementedException();
        }
    }
}
