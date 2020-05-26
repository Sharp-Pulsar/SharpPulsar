using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;

namespace SharpPulsar.Handlers
{
    public class DefaultConsumerEventListener : IConsumerEventListener
    {
        public Action<object> Logs;

        public DefaultConsumerEventListener(Action<object> log)
        {
            Logs = log;
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
        
    }
}
