namespace SharpPulsar.Messages.Consumer
{
    public sealed class SeekTimestamp
    {
        public long Timestamp { get; }
        public SeekTimestamp(long timestamp)
        {
            Timestamp = timestamp;
        }
    } 
}
