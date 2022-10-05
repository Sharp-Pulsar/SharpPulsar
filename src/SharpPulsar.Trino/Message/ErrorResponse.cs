using SharpPulsar.Trino.Trino;

namespace SharpPulsar.Trino.Message
{
    public class ErrorResponse : IQueryResponse
    {
        public ErrorResponse(QueryError error, List<Warning> warnings)
        {
            Error = error;
            Warnings = warnings;
        }

        public QueryError Error { get; }
        public List<Warning> Warnings { get; }
    }
}
