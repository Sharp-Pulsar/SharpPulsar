namespace SharpPulsar.Messages.Producer
{
    public record GetProducerName
    {
        public static GetProducerName Instance = new GetProducerName();
    }
    
}
