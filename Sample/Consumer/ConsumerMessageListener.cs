﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl;

namespace Samples.Consumer
{
    public class ConsumerMessageListener:IMessageListener
    {
        public void Received(IActorRef consumer, IMessage msg)
        {
            var students = msg.ToTypeOf<Students>();
            Console.WriteLine($"Consumer >> {students.Name}");
            if (msg.MessageId is MessageId m)
            {
                consumer.Tell(new AckMessage(new MessageIdReceived(m.LedgerId, m.EntryId, -1, m.PartitionIndex)));
                Console.WriteLine($"Consumer >> {students.Name}- partition: {m.PartitionIndex}");
            }
            else if (msg.MessageId is BatchMessageId b)
            {
                consumer.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
            }
            else
             Console.WriteLine($"Unknown messageid: {msg.MessageId.GetType().Name}");

        }
    }
}
