

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(MultiTopicCollection), DisableParallelization = true)]
    public class MultiTopicCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
