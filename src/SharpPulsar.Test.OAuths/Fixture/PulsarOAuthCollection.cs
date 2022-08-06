

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(PulsarOAuthCollection), DisableParallelization = true)]
    public class PulsarOAuthCollection : ICollectionFixture<PulsarOAuthFixture>
    {

    }
}
