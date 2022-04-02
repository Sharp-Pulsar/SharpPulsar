

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(IntegrationCollection), DisableParallelization = true)]
    public class IntegrationCollection : ICollectionFixture<PulsarFixture> 
    { 
        
    }
}
