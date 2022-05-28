

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(PulsarTlsCollection), DisableParallelization = true)]
    public class PulsarTlsCollection : ICollectionFixture<PulsarFixture>
    {
    }
}
