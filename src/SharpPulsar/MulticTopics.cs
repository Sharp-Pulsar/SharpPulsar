using System;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar
{
    public class MulticTopics<T>: Consumer<T>
    {
        private readonly IActorRef _consumerActor;
        public MulticTopics(IActorRef stateActor, IActorRef consumer, ISchema<T> schema, ConsumerConfigurationData<T> conf, TimeSpan operationTimeout)
            : base(stateActor, consumer, schema, conf, operationTimeout)
        {
            _consumerActor = consumer;
        }
       
        public void Unsubscribe(string topicName)
        {
            UnsubscribeAsync(topicName).GetAwaiter().GetResult();
        }
        public async ValueTask UnsubscribeAsync(string topicName)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new UnsubscribeTopicName(topicName))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
    }
}
