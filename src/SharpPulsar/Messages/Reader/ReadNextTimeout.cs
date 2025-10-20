
using System;

namespace SharpPulsar.Messages.Reader
{
    public record ReadNextTimeout
    {
        public long Timeout { get; } 
        public ReadNextTimeout(TimeSpan timeout)
        {
            Timeout = (long)timeout.TotalSeconds;
        }
    }
}
