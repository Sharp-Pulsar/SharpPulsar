using SharpPulsar.TestContainer;
using Xunit;

namespace SharpPulsar.Sql.Tests
{
    [CollectionDefinition(nameof(IntegrationCollection), DisableParallelization = true)]
    public class IntegrationCollection : ICollectionFixture<PulsarFixture> { }
}
