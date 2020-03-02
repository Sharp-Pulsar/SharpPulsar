using System;
using System.Collections.Generic;
using Akka.Actor;
using Producer;
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
            var m = (BatchMessageId)msg.MessageId;
            if(msg.Value is Students students)
                Console.WriteLine($"{msg.TopicName} >> {students.Name}");
            consumer.Tell(new AckMessage(new MessageIdReceived(m.LedgerId, m.EntryId, m.BatchIndex, m.PartitionIndex)));
        }
    }
}
