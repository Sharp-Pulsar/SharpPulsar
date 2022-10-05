using SharpPulsar.Trino.Trino;

namespace SharpPulsar.Trino.Message
{
    public class StatsResponse : IQueryResponse
    {
        public StatsResponse(StatementStats stats)
        {
            Stats = stats;
        }

        public StatementStats Stats { get; }
    }
}
