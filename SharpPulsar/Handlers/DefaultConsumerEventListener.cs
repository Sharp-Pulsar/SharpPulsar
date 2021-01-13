using System;
using Akka.Actor;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Api;

namespace SharpPulsar.Handlers
{
    public class DefaultConsumerEventListener : IConsumerEventListener
    {
        public Action<object> Logs;
        public Action<CreatedConsumer> CreatedAction;

        public DefaultConsumerEventListener(Action<object> log)
        {
            Logs = log;
        }
        public DefaultConsumerEventListener(Action<CreatedConsumer> created, Action<object> log)
        {
            Logs = log;
            CreatedAction = created;
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

        public void Created(CreatedConsumer created)
        {
            CreatedAction?.Invoke(created);
        }
    }
}
