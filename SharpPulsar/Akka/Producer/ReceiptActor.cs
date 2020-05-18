using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Producer
{
    public class ReceiptActor : ReceiveActor, IWithUnboundedStash
    {
        public IStash Stash { get; set; }
        private readonly IProducerEventListener _listener;
        private readonly int _index;

        public ReceiptActor(IProducerEventListener listener, int index)
        {
            _listener = listener;
            _index = index;

            Receive<SentReceipt>(s =>
            {
                var ns = new SentReceipt(s.ProducerId, s.SequenceId, s.EntryId, s.LedgerId, s.BatchIndex, _index);
                _listener.MessageSent(ns);
            });
        }

        public static Props Prop(IProducerEventListener listener, int index)
        {
            return Props.Create(() => new ReceiptActor(listener, index));
        }
    }
}
