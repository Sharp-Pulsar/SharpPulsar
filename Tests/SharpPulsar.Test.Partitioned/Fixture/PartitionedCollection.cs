

namespace SharpPulsar.Test.Partitioned.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(PartitionedCollection), DisableParallelization = true)]
    public class PartitionedCollection : ICollectionFixture<PulsarFixture>
    {
    }
}
