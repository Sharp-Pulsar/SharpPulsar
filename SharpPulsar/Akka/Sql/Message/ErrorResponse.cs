using System.Collections.Generic;
using SharpPulsar.Presto;

namespace SharpPulsar.Akka.Sql.Message
{
    public class ErrorResponse: IQueryResponse
    {
        public ErrorResponse(QueryError error, List<PrestoWarning> warnings)
        {
            Error = error;
            Warnings = warnings;
        }

        public QueryError Error { get; }
        public List<PrestoWarning> Warnings { get; }
    }
}
