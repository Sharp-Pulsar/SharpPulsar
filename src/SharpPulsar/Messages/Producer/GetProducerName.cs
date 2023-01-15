namespace SharpPulsar.Messages.Producer
{
    public readonly record struct GetProducerName
    {
        public static GetProducerName Instance = new GetProducerName();
    }
    
}
