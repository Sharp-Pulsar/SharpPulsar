using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Presto;

namespace SharpPulsar.Sql.Message
{
    public sealed class SqlQuery
    {
        public SqlQuery(ClientOptions options)
        {
            ClientOptions = options;
        }
        
        public ClientOptions ClientOptions { get; }
    }
    internal sealed class SqlSession 
    {
        public SqlSession(ClientSession session, ClientOptions options)
        {
            ClientSession = session;
            ClientOptions = options;
        }
        
        public ClientSession ClientSession { get; }
        public ClientOptions ClientOptions { get; }
    }
}
