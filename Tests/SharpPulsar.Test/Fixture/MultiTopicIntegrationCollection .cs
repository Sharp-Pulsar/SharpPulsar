

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(MultiTopicIntegrationCollection), DisableParallelization = true)]
    public class MultiTopicIntegrationCollection : ICollectionFixture<PulsarMultiTopicFixture> 
    { 
        
    }
}
