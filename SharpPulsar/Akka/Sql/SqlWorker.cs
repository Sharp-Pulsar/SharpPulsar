using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Sql.Client;
using SharpPulsar.Akka.Sql.Message;

namespace SharpPulsar.Akka.Sql
{
    public class SqlWorker: ReceiveActor
    {
        public SqlWorker(IActorRef pulsarManager)
        {
            Receive<SqlSession>(Query);
            Receive<IQueryResponse>(q => { pulsarManager.Tell(new SqlData(q)); });

        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Query(SqlSession query)
        {
            try
            {
                var q = query;
                var executor = new Executor(q.ClientSession, q.ClientOptions, Self, Context.System.Log);
                q.Log($"Executing: {q.ClientOptions.Execute}");
                executor.Run();
            }
            catch (Exception ex)
            {
                query.ExceptionHandler(ex);
            }
        }
        public static Props Prop(IActorRef pulsarManager)
        {
            return Props.Create(() => new SqlWorker(pulsarManager));
        }
    }
}
