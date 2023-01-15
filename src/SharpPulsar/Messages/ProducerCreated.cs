namespace SharpPulsar.Messages
{
    public readonly record struct ProducerCreated
    {
        public ProducerCreated(string name, long requestId, long lastSequenceId, byte[] schemaVersion)
        {
            Name = name;
            RequestId = requestId;
            LastSequenceId = lastSequenceId;
            SchemaVersion = schemaVersion;
        }

        public string Name { get; }
        public long RequestId { get; }
        public long LastSequenceId { get; }
        public byte[] SchemaVersion { get; }
    }

}
