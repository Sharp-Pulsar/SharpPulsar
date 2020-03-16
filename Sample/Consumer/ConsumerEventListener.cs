using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;

namespace Samples.Consumer
{
    public class ConsumerEventListener:IConsumerEventListener
    {
        private Dictionary<string, IActorRef> _actorRefs;

        public ConsumerEventListener()
        {
                _actorRefs = new Dictionary<string, IActorRef>();
        }
        public void BecameActive(string consumer, int partitionId)
        {
            Console.WriteLine(partitionId);
        }

        public void BecameInactive(string consumer, int partitionId)
        {
            Console.WriteLine(partitionId);
        }

        public void Error(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        public void Log(string log)
        {
            Console.WriteLine(log);
        }

        public IActorRef GetConsumer(string topic)
        {
            if (_actorRefs.ContainsKey(topic))
                return _actorRefs[topic];
            return null;
        }
        public void ConsumerCreated(CreatedConsumer consumer)
        {
            Console.WriteLine($"Consumer for topic: {consumer.Topic}");
            _actorRefs[consumer.Topic] =  consumer.Consumer;
        }

        public void LastMessageId(LastMessageIdReceived received)
        {
            throw new NotImplementedException();
        }
    }
}
