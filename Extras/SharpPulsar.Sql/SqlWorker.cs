using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.Actor;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;

namespace SharpPulsar.Sql
{
    public class SqlWorker : ReceiveActor
    {

        private readonly BufferBlock<SqlData> _buffer;
        public SqlWorker()
        {
            _buffer = new BufferBlock<SqlData>();
            ReceiveAsync<SqlSession>(async s=> await Query(s));
            Receive<IQueryResponse>(q =>
            {
                _buffer.Post(new SqlData(q));
            });
            Receive<Read>(_ =>
            {
                _buffer.TryReceive(out var data);
                Sender.Tell(data);
            });
        }

        protected override void Unhandled(object message)
        {

        }

        private async ValueTask Query(SqlSession query)
        {
            try
            {
                var q = query;
                var executor = new Executor(q.ClientSession, q.ClientOptions, Self, Context.System.Log);
                q.Log($"Executing: {q.ClientOptions.Execute}");
                await executor.Run();
            }
            catch (Exception ex)
            {
                query.ExceptionHandler(ex);
                Context.System.Log.Error(ex.ToString());
            }
        }
        public static Props Prop()
        {
            return Props.Create(() => new SqlWorker());
        }
    }
}
