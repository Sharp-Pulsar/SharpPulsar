

namespace SharpPulsar.Test.SQL
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(SqlCollection), DisableParallelization = true)]
    public class SqlCollection : ICollectionFixture<PulsarFixture> { }
}
