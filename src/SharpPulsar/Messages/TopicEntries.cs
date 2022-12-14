namespace SharpPulsar.Messages
{
    public readonly record struct TopicEntries
    {
        public TopicEntries(long? entries)
        {
            Entries = entries;
        }

        public long? Entries { get; }
    }
}
