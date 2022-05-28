﻿

namespace SharpPulsar.Test.Fixture
{
    using SharpPulsar.TestContainer;
    using Xunit;

    [CollectionDefinition(nameof(TableViewCollection), DisableParallelization = true)]
    public class TableViewCollection : ICollectionFixture<PulsarFixture>
    {
        
    }
}
