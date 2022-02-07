using SharpPulsar.TestContainer;
using Xunit;

namespace SharpPulsar.Test.EventSourcing
{
    [CollectionDefinition(nameof(EventCollection), DisableParallelization = true)]
    public class EventCollection : ICollectionFixture<PulsarFixture> { }
}
