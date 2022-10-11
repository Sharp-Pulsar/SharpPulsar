

namespace SharpPulsar.Test.Token.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(PulsarTokenCollection), DisableParallelization = true)]
    public class PulsarTokenCollection : ICollectionFixture<PulsarTokenFixture>
    {

    }
}
