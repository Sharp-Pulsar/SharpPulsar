using System;
using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Sql
{
    public class SqlManager : ReceiveActor, IWithUnboundedStash
    {
        public SqlManager()
        {
               Become(Init); 
        }

        private void Init()
        {
            Receive<SqlServers>(servers =>
            {
                foreach (var s in servers.Servers)
                {
                    var srv = Regex.Replace(s, @"[^\w\d]", "");
                    Context.ActorOf(SqlWorker.Prop(s), srv);
                }
                Become(Ready);
                Stash.UnstashAll();
            });
            ReceiveAny(_=> Stash.Stash());
        }

        private void Ready()
        {
            Receive<QueryData>(q =>
            {
                var srv = Regex.Replace(q.DestinationServer, @"[^\w\d]", "");
                var dest = Context.Child(srv);
                if(dest.IsNobody())
                    q.ExceptionHandler(new Exception($"unknown destination server: {q.DestinationServer}"));
                else
                    dest.Tell(q);

            });
        }

        protected override void Unhandled(object message)
        {
            
        }

        public static Props Prop()
        {
            return Props.Create(() => new SqlManager());
        }
        public IStash Stash { get; set; }
    }
}
