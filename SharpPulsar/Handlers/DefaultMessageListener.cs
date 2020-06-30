using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Api;

namespace SharpPulsar.Handlers
{
    public class DefaultMessageListener : IMessageListener, IReaderListener
    {
        public Action<IActorRef, IMessage, IList<long>> Consumer;
        public Action<IMessage> Reader;

        public DefaultMessageListener(Action<IActorRef, IMessage, IList<long>> consumer, Action<IMessage> reader)
        {
            Consumer = consumer;
            Reader = reader;
        }
        public void Received(IActorRef consumer, IMessage msg, IList<long> ackSets)
        {
            Consumer?.Invoke(consumer, msg, ackSets);
        }

        public void Received(IMessage msg)
        {
            Reader?.Invoke(msg);
        }
    }
}
