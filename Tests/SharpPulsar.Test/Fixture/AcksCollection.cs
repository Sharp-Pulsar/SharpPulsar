﻿

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(AcksCollection), DisableParallelization = true)]
    public class AcksCollection : ICollectionFixture<PulsarFixture>
    {
        

    }
}
