using BAMCIS.Util.Concurrent;

namespace SharpPulsar.Messages.Reader
{
    public sealed class ReadNextTimeout
    {
        public int Timeout { get; } 
        public TimeUnit Unit { get; }
        public ReadNextTimeout(int timeout, TimeUnit unit)
        {
            Timeout = timeout;
            Unit = unit;
        }
    }
}
