using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;

namespace SharpPulsar.Handlers
{
    public class DefaultConsumerEventListener : IConsumerEventListener
    {
        public Action<object> Logs;
        public Action<string, IActorRef> Consumers;
        public Action<string, LastMessageIdResponse> LastMessageIds;

        public DefaultConsumerEventListener(Action<object> log, Action<string, IActorRef> consumers, Action<string, LastMessageIdResponse> lastMessageIds)
        {
            Logs = log;
            Consumers = consumers;
            LastMessageIds  = lastMessageIds;
        }
        public void BecameActive(string consumer, int partitionId)
        {

        }

        public void BecameInactive(string consumer, int partitionId)
        {

        }

        public void Error(Exception ex)
        {
            Logs.Invoke(ex.ToString());
        }

        public void Log(string log)
        {
           Logs.Invoke(log);
        }

        public void ConsumerCreated(CreatedConsumer consumer)
        {
            Logs.Invoke($"Consumer for topic: {consumer.Topic}");
            Consumers.Invoke(consumer.Topic, consumer.Consumer);
        }

        public void LastMessageId(LastMessageIdReceived received)
        {
            LastMessageIds.Invoke(received.Topic, received.Response);
        }
    }
}
