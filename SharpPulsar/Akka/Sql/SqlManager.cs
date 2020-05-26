using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Sql.Live;

namespace SharpPulsar.Akka.Sql
{
    public class SqlManager : ReceiveActor, IWithUnboundedStash
    {
        public SqlManager(IActorRef pulsarManager)
        {
            Receive((InternalCommands.Sql q) =>
            {
                var srv = Regex.Replace(q.Server, @"[^\w\d]", "");
                var dest = Context.Child(srv);
                if (dest.IsNobody())
                    dest = Context.ActorOf(SqlWorker.Prop(q.Server, pulsarManager), srv);
                dest.Tell(q);
            });

            Receive((LiveSql q) =>
            {
                var srv = Regex.Replace(q.Server, @"[^\w\d]", "");
                var dest = Context.Child(srv);
                if (dest.IsNobody())
                    dest = Context.ActorOf(LiveQueryCoordinator.Prop(pulsarManager));
                dest.Tell(q);

            });
        }
        
        protected override void Unhandled(object message)
        {
            
        }

        public static Props Prop(IActorRef pulsarManager)
        {
            return Props.Create(() => new SqlManager(pulsarManager));
        }
        public IStash Stash { get; set; }
    }
}
