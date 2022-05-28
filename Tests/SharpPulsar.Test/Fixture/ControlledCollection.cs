

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(ControlledCollection), DisableParallelization = true)]
    public class ControlledCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
