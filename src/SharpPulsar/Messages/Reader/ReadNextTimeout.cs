
using System;

namespace SharpPulsar.Messages.Reader
{
    public readonly record struct ReadNextTimeout
    {
        public long Timeout { get; } 
        public ReadNextTimeout(TimeSpan timeout)
        {
            Timeout = (long)timeout.TotalSeconds;
        }
    }
}
