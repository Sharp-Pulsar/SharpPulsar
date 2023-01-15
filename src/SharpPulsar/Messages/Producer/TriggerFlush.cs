
namespace SharpPulsar.Messages.Producer
{
    public readonly record struct TriggerFlush
    {
        public static TriggerFlush Instance = new TriggerFlush();
    }
}
