using SharpPulsar.Trino.Message;

namespace SharpPulsar.Trino
{
    public sealed class SqlData
    {
        public SqlData(IQueryResponse response)
        {
            Response = response;
        }
        public IQueryResponse Response { get; }
    }
}
