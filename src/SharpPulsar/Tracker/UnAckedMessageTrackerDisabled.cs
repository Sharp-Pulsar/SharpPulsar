using Akka.Actor;
using Akka.Event;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTrackerDisabled<T> : ReceiveActor
    {
        public UnAckedMessageTrackerDisabled()
        {
            Receive<Clear>(c => { });
            Receive<Remove>(c =>
            {
                //Sender.Tell(true);
            });
            Receive<RemoveMessagesTill>(c => { });
            Receive<Add<T>>(c => { });
            Receive<Size>(c => { });
            ReceiveAny(_ =>
            {
                //no ops
            });
        }

        protected override void Unhandled(object message)
        {
            Context.GetLogger().Warning(message.ToString());
        }

        public static Props Prop()
        {
            return Props.Create(()=> new UnAckedMessageTrackerDisabled<T>());
        }
    }
}
