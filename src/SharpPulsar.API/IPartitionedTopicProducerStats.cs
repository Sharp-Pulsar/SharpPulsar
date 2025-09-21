namespace SharpPulsar.API
{
    /// <summary>
    /// Partitioned topic Producer statistics recorded by client.
    /// 
    /// <para>All the stats are relative to the last recording period. The interval of the stats refreshes is configured with
    /// <seealso cref="ClientBuilder.statsInterval(long, java.util.concurrent.TimeUnit)"/> with a default of 1 minute.
    /// </para>
    /// </summary>
    internal interface IPartitionedTopicProducerStats : IProducerStats
    {
        /// <returns> stats for each partition if topic is partitioned topic </returns>
        IDictionary<string, IProducerStats> GetPartitionStats();
    }
}
