using Akka.Actor;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTrackerDisabled: ReceiveActor
    {
        public UnAckedMessageTrackerDisabled()
        {
            Receive<Clear>(c => { });
            Receive<Remove>(c =>
            {
                Sender.Tell(true);
            });
            Receive<RemoveMessagesTill>(c => { Sender.Tell(0);});
            Receive<Add>(c => { Sender.Tell(true);});
            Receive<Size>(c => { Sender.Tell(0L);});
        }
        public static Props Prop()
        {
            return Props.Create(()=> new UnAckedMessageTrackerDisabled());
        }
    }
}
