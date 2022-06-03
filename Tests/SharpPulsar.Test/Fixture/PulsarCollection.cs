

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(PulsarCollection), DisableParallelization = true)]
    public class PulsarCollection : ICollectionFixture<PulsarFixture> 
    {
       
    }
}
