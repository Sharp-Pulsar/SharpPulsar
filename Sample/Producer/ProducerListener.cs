using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Samples.Producer
{
    public class ProducerListener: IProducerEventListener
    {
        private Dictionary<string, IActorRef> _actorRefs;

        public ProducerListener()
        {
            _actorRefs = new Dictionary<string, IActorRef>();
        }

        public void ProducerCreated(CreatedProducer producer)
        {
            Console.WriteLine($"Producer for topic: {producer.Topic}");
            _actorRefs.Add(producer.Topic, producer.Producer);
        }

        public void MessageSent(SentReceipt receipt)
        {
            Console.WriteLine(JsonSerializer.Serialize(receipt));
        }

        public void Log(object log)
        {
            Console.WriteLine(JsonSerializer.Serialize(log));
        }

        public IActorRef GetProducer(string topic)
        {
            if(_actorRefs.ContainsKey(topic))
                return _actorRefs[topic];
            return null;
        }
    }
}
