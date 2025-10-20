
namespace SharpPulsar.Messages.Producer
{
    public record TriggerFlush
    {
        public static TriggerFlush Instance = new TriggerFlush();
    }
}
