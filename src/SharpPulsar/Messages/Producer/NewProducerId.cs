
namespace SharpPulsar.Messages.Producer
{
    public readonly record struct NewProducerId
    {
        public static NewProducerId Instance = new NewProducerId();
    }
}
