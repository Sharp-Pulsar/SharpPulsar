
namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct Clear
    {
        public static Clear Instance = new Clear();
    }
    public readonly record struct TestClear
    {
        public static TestClear Instance = new TestClear();
    }
}
