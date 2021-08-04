using SharpPulsar.Sql.Message;

namespace SharpPulsar.Sql
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
