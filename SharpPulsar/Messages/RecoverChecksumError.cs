
using Akka.Actor;

namespace SharpPulsar.Messages
{
    public sealed class RecoverChecksumError
    {
        public long SequenceId { get; }
        public IActorRef ClientCnx { get; }
        public RecoverChecksumError(IActorRef clientCnx, long sequenceId)
        {
            SequenceId = sequenceId;
            ClientCnx = clientCnx;
        }
    }
    public sealed class Terminated
    {
        public IActorRef ClientCnx { get; }
        public Terminated(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public sealed class RecoverNotAllowedError
    {
        public long SequenceId { get; }
        public RecoverNotAllowedError(long sequenceId)
        {
            SequenceId = sequenceId;
        }
    }
}
