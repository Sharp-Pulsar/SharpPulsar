

namespace SharpPulsar.Test.Fixture
{
    using System;
    using SharpPulsar.Builder;
    using SharpPulsar.TestContainer;
    using SharpPulsar.User;
    using Xunit;

    [CollectionDefinition(nameof(PulsarCollection), DisableParallelization = true)]
    public class PulsarCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
