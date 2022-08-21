using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;

namespace SharpPulsar.Sql.Public
{
    public class SqlInstance
    {
        private readonly IActorRef _queryActor;
        public readonly ClientOptions ClientOptions;
        public SqlInstance(ActorSystem system, ClientOptions clientOptions)
        {
            _queryActor = system.ActorOf(SqlWorker.Prop());
            ClientOptions = clientOptions;
        }
        
        public SqlData Execute(TimeSpan? timeout = null, string query = "")
        {
            return ExecuteAsync(timeout, query).GetAwaiter().GetResult();
        }
        
        public async ValueTask<SqlData> ExecuteAsync(TimeSpan? timeout = null, string query = "")
        {
            if (string.IsNullOrWhiteSpace(ClientOptions.Server))
                throw new ArgumentException("Trino Server cannot be empty");

            var hasQuery = !string.IsNullOrWhiteSpace(ClientOptions.Execute) || (!string.IsNullOrWhiteSpace(query) || !string.IsNullOrWhiteSpace(ClientOptions.File));

            if (!hasQuery)
                throw new ArgumentException("Query cannot be empty");

            if (!string.IsNullOrWhiteSpace(ClientOptions.Execute))
            {
                ClientOptions.Execute.TrimEnd(';');
            }
            else if(!string.IsNullOrWhiteSpace(query))
            {
                ClientOptions.Execute = query.TrimEnd(';');
            }
            else
            {
                ClientOptions.Execute =  await File.ReadAllTextAsync(ClientOptions.File);
                ClientOptions.Execute.TrimEnd(';');
            }
            var q = new SqlSession(ClientOptions.ToClientSession(), ClientOptions);

            var ask = timeout.HasValue
                ? await _queryActor.Ask<AskResponse>(q, timeout.Value)
                : await _queryActor.Ask<AskResponse>(q);

            if (ask.Failed)
                throw ask.Exception;

            return ask.ConvertTo<SqlData>();
        }
        
    }
}
