﻿using System;
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

        public void ConsumerCreated(CreatedConsumer consumer)
        {
            Console.WriteLine($"Producer for topic: {consumer.Topic}");
            _actorRefs.Add(consumer.Topic, consumer.Consumer);
        }
    }
}