

namespace SharpPulsar.Test.Tls.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(TlsCollection), DisableParallelization = true)]
    public class TlsCollection : ICollectionFixture<PulsarFixture>
    {
    }
}
