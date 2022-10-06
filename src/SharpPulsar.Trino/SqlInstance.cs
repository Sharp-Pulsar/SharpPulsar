using Akka.Actor;
using SharpPulsar.Trino.Message;

namespace SharpPulsar.Trino
{
    public class SqlInstance
    {
        private readonly IActorRef _queryActor;
        private readonly ClientOptions _clientOptions;
        public SqlInstance(ActorSystem system, ClientOptions clientOptions)
        {
            _queryActor = system.ActorOf(SqlWorker.Prop());
            _clientOptions = clientOptions;
        }

        public SqlData Execute(TimeSpan? timeout = null, string query = "")
        {
            return ExecuteAsync(timeout, query).GetAwaiter().GetResult();
        }

        public async ValueTask<SqlData> ExecuteAsync(TimeSpan? timeout = null, string query = "")
        {
            if (string.IsNullOrWhiteSpace(_clientOptions.Server))
                throw new ArgumentException("Trino Server cannot be empty");

            var hasQuery = !string.IsNullOrWhiteSpace(_clientOptions.Execute) || !string.IsNullOrWhiteSpace(query) || !string.IsNullOrWhiteSpace(_clientOptions.File);

            if (!hasQuery)
                throw new ArgumentException("Query cannot be empty");

            if (!string.IsNullOrWhiteSpace(_clientOptions.Execute))
            {
                _clientOptions.Execute.TrimEnd(';');
            }
            else if (!string.IsNullOrWhiteSpace(query))
            {
                _clientOptions.Execute = query.TrimEnd(';');
            }
            else
            {
                _clientOptions.Execute = await File.ReadAllTextAsync(_clientOptions.File);
                _clientOptions.Execute.TrimEnd(';');
            }
            var q = new SqlSession(_clientOptions.ToClientSession(), _clientOptions);

            var ask = timeout.HasValue
                ? await _queryActor.Ask<AskResponse>(q, timeout.Value)
                : await _queryActor.Ask<AskResponse>(q);

            if (ask.Failed)
                throw ask.Exception;

            return ask.ConvertTo<SqlData>();
        }

    }
}
