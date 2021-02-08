using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Messages;
using SharpPulsar.Sql.Live;

namespace SharpPulsar.Sql
{
    public class SqlManager : ReceiveActor, IWithUnboundedStash
    {
        public SqlManager(IActorRef pulsarManager)
        {
            Receive((SqlSession q) =>
            {
                var srv = Regex.Replace(q.ClientSession.Server.Host, @"[^\w\d]", "");
                var dest = Context.Child(srv);
                if (dest.IsNobody())
                    dest = Context.ActorOf(SqlWorker.Prop(pulsarManager), srv);
                dest.Tell(q);
            });

            Receive((LiveSqlSession q) =>
            {
                var srv = Regex.Replace(q.ClientSession.Server.Host, @"[^\w\d]", "");
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
