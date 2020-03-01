using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Impl;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class ConsumedMessage
    {
        public ConsumedMessage(IActorRef consumer, MessageImpl message)
        {
            Consumer = consumer;
            Message = message;
        }

        public IActorRef Consumer { get; }
        public MessageImpl Message { get; }
    }
}
