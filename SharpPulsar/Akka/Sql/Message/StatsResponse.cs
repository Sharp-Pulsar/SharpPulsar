using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Presto;

namespace SharpPulsar.Akka.Sql.Message
{
    public class StatsResponse:IQueryResponse
    {
        public StatsResponse(StatementStats stats)
        {
            Stats = stats;
        }

        public StatementStats Stats { get; }
    }
}
