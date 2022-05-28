

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(AutoCollection), DisableParallelization = true)]
    public class AutoCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
