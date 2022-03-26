

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer.ServiceProviderFixtures;
    using Xunit;

    [CollectionDefinition(nameof(ControlledCollection), DisableParallelization = true)]
    public class ControlledCollection : ICollectionFixture<ControlledClusterFailoverFixture> { }
}
