
using Akka.Actor;

namespace SharpPulsar.Messages
{
    public readonly record struct RecoverChecksumError
    {
        public long SequenceId { get; }
        public IActorRef ClientCnx { get; }
        public RecoverChecksumError(IActorRef clientCnx, long sequenceId)
        {
            SequenceId = sequenceId;
            ClientCnx = clientCnx;
        }
    }
    public readonly record struct Terminated
    {
        public IActorRef ClientCnx { get; }
        public Terminated(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public readonly record struct RecoverNotAllowedError
    {
        public long SequenceId { get; }
        public string ErrorMsg { get; }
        public RecoverNotAllowedError(long sequenceId, string errorMsg)
        {
            SequenceId = sequenceId;
            ErrorMsg = errorMsg;   
        }
    }
}
