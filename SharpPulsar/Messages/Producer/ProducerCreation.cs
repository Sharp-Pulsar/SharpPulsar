using SharpPulsar.Common.Entity;
using System;

namespace SharpPulsar.Messages.Producer
{

    public sealed class ProducerCreation
    {
        public bool Errored { get; } = false;
        public Exception Exception { get; }
        public ProducerResponse Producer { get; }
        public ProducerCreation(ProducerResponse producer)
        {
            Producer = producer;
        }
        public ProducerCreation(Exception exception)
        {
            Errored = true;
            Exception = exception;
        }
    }
    public sealed class TriggerFlush
    {
        public static TriggerFlush Instance = new TriggerFlush();
    }
}
