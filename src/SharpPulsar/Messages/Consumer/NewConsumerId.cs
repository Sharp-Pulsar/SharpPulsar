
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct NewConsumerId
    {
        public static NewConsumerId Instance = new NewConsumerId();
    }
}
