
namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct FlushAndClean
    {
        public static FlushAndClean Instance = new FlushAndClean();
    }
}
