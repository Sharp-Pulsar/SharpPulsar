

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(TransactionCollection), DisableParallelization = true)]
    public class TransactionCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
