using Akka.Actor;
using Akka.Event;
using SharpPulsar.Trino.Message;

namespace SharpPulsar.Trino
{
    public class SqlWorker : ReceiveActor
    {
        private IActorRef _sender;
        private readonly ILoggingAdapter _log;
        public SqlWorker()
        {
            _log = Context.GetLogger();
            ReceiveAsync<SqlSession>(async s =>
            {
                _sender = Sender;
                await Query(s);
            });
            Receive<IQueryResponse>(q =>
            {
                _sender.Tell(new AskResponse(new SqlData(q)));
            });
        }

        private async ValueTask Query(SqlSession query)
        {
            try
            {
                var q = query;
                var executor = new Executor(q.ClientSession, q.ClientOptions, Self, Context.System.Log);
                _log.Info($"Executing: {q.ClientOptions.Execute}");
                await executor.Run();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                _sender.Tell(new AskResponse(ex));
            }
        }
        public static Props Prop()
        {
            return Props.Create(() => new SqlWorker());
        }
    }
}
