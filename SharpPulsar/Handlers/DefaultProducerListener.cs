using System;
using System.Collections.Generic;
using System.Text.Json;
using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;

namespace SharpPulsar.Handlers
{
    public class DefaultProducerListener : IProducerEventListener
    {
        private readonly Action<string, string, IActorRef> _producers;
        private readonly Action<string> _receipts;
        private readonly Action<object> _logs;
        private static readonly Dictionary<string, IActorRef> ExistingActors = new Dictionary<string, IActorRef>();

        public DefaultProducerListener(Action<object> log, Action<string, string, IActorRef> producers, Action<string> receipts)
        {
            _producers = producers;
            _receipts = receipts;
            _logs = log;
        }

        /*public void ProducerCreated(CreatedProducer producer)
        {
            if(producer == null)
                return;
            if (producer.Producer != null)
            {
                ExistingActors[producer.Topic] = producer.Producer;
                _producers(producer.Topic, producer.Name, producer.Producer);
                var s = $"Producer {producer.Name} Created for [{producer.Topic}]";
                Log(s);
            }
            else
            {
                _producers(producer.Topic, producer.Name, ExistingActors[producer.Topic]);
                var s = $"Producer {producer.Name} exists for [{producer.Topic}]";
                Log(s);
            }
        }*/

        public void MessageSent(SentReceipt receipt)
        {
            var json = JsonSerializer.Serialize(receipt);
            _receipts.Invoke(json);
            var s = $"Receipt Added [{json}]";
            Log(s);
        }

        public void Log(object log)
        {
            _logs.Invoke(log);
        }

    }
}
