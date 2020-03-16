using System;
using System.Text.Json;
using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;

namespace SharpPulsar.Handlers
{
    public class DefaultProducerListener : IProducerEventListener
    {
        public Action<string, IActorRef> Producers;
        public Action<string> Receipts;
        public Action<object> Logs;

        public DefaultProducerListener(Action<object> log, Action<string, IActorRef> producers, Action<string> receipts)
        {
            Producers = producers;
            Receipts = receipts;
            Logs = log;
        }

        public void ProducerCreated(CreatedProducer producer)
        {
            Producers.Invoke(producer.Topic, producer.Producer);
            var s = $"Producer Created [{producer.Topic}]";
            Log(s);
        }

        public void MessageSent(SentReceipt receipt)
        {
            var json = JsonSerializer.Serialize(receipt);
            Receipts.Invoke(json);
            var s = $"Receipt Added [{json}]";
            Log(s);
        }

        public void Log(object log)
        {
            Logs.Invoke(log);
        }

    }
}
