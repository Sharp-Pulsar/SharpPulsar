
namespace SharpPulsar.Messages
{
    public sealed class RecoverChecksumError
    {
        public long SequenceId { get; }
        public ClientCnx ClientCnx { get; }
        public RecoverChecksumError(ClientCnx clientCnx, long sequenceId)
        {
            SequenceId = sequenceId;
            ClientCnx = clientCnx;
        }
    }
    public sealed class Terminated
    {
        public ClientCnx ClientCnx { get; }
        public Terminated(ClientCnx clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
}
