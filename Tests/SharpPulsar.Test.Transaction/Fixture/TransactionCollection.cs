
using SharpPulsar.TestContainer;
using Xunit;

namespace SharpPulsar.Test.Transaction.Fixture
{
    [CollectionDefinition(nameof(TransactionCollection), DisableParallelization = true)]
    public class TransactionCollection : ICollectionFixture<PulsarFixture> { }
}
