

namespace SharpPulsar.Test.Fixture
{
    using System.Threading.Tasks;
    using System;
    using SharpPulsar.TestContainer;
    using Xunit;
    using Xunit.Abstractions;

    [CollectionDefinition(nameof(PulsarCollection), DisableParallelization = true)]
    public class PulsarCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
