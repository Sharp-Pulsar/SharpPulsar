using System;
using Akka.Actor;
using SharpPulsar.Api;

namespace SharpPulsar.Handlers
{
    public class DefaultMessageListener : IMessageListener, IReaderListener
    {
        public Action<IActorRef, IMessage> Consumer;
        public Action<IMessage> Reader;

        public DefaultMessageListener(Action<IActorRef, IMessage> consumer, Action<IMessage> reader)
        {
            Consumer = consumer;
            Reader = reader;
        }
        public void Received(IActorRef consumer, IMessage msg)
        {
            Consumer.Invoke(consumer, msg);
        }

        public void Received(IMessage msg)
        {
            Reader.Invoke(msg);
        }
    }
}
