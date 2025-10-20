namespace SharpPulsar.Messages
{
    public record TopicEntries
    {
        public TopicEntries(long? entries)
        {
            Entries = entries;
        }

        public long? Entries { get; }
    }
}
