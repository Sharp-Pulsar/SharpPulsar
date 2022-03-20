

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer.ServiceProviderFixtures;
    using Xunit;

    [CollectionDefinition(nameof(AutoCollection), DisableParallelization = true)]
    public class AutoCollection : ICollectionFixture<AutoClusterFailoverFixture> { }
}
