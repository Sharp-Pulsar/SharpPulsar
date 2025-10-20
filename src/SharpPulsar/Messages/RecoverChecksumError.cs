
using Akka.Actor;

namespace SharpPulsar.Messages
{
    public record RecoverChecksumError
    {
        public long SequenceId { get; }
        public IActorRef ClientCnx { get; }
        public RecoverChecksumError(IActorRef clientCnx, long sequenceId)
        {
            SequenceId = sequenceId;
            ClientCnx = clientCnx;
        }
    }
    public record Terminated
    {
        public IActorRef ClientCnx { get; }
        public Terminated(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public record RecoverNotAllowedError
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
