using SharpPulsar.Presto;

namespace SharpPulsar.Sql.Message
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
