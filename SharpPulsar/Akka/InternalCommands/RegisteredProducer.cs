using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public class RegisteredProducer
    {
        public RegisteredProducer(long producerId, string producerName, string topic)
        {
            ProducerId = producerId;
            ProducerName = producerName;
            Topic = topic;
        }

        public long ProducerId { get; }
        public string ProducerName { get; }
        public string Topic { get; }
    }
}
