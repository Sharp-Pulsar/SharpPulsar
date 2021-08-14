namespace SharpPulsar.Messages.Consumer
{
    public sealed class AckReceipt
    {
       public long RequestId { get; }
        public AckReceipt(long requestid)
        {
            RequestId = requestid;
        }
    }
}
