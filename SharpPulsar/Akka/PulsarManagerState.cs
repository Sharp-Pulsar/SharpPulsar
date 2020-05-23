using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;

namespace SharpPulsar.Akka
{
    public class PulsarManagerState
    {

        public BlockingQueue<CreatedConsumer> ConsumerQueue { get; set; }
        public BlockingQueue<CreatedProducer> ProducerQueue { get; set; }
    }
}
