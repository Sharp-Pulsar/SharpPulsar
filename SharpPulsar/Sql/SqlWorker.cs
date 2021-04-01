using System;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Messages;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;

namespace SharpPulsar.Sql
{
    public class SqlWorker : ReceiveActor
    {
        public SqlWorker(SqlQueue<SqlData> queue)
        {
            ReceiveAsync<SqlSession>(async s=> await Query(s));
            Receive<IQueryResponse>(q =>
            {
                queue.Post(new SqlData(q));
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
        public static Props Prop(SqlQueue<SqlData> queue)
        {
            return Props.Create(() => new SqlWorker(queue));
        }
    }
}
