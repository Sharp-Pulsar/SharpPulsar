

namespace SharpPulsar.Test.EventSource.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(EventSourceCollection), DisableParallelization = true)]
    public class EventSourceCollection : ICollectionFixture<PulsarFixture>
    {
        

    }
}
