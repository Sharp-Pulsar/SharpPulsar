
namespace SharpPulsar.Messages.Consumer
{
    public sealed class Receive
    {
        public static Receive Instance = new Receive();
    } 
    public sealed class BatchReceive
    {
        public static BatchReceive Instance = new BatchReceive();
    }
    public sealed class ReceiveWithTimeout
    {
        public int Timeout { get; }
        public ReceiveWithTimeout(int timeout)
        {
            Timeout = timeout;
        }
    }
    public sealed class BatchReceiveWithTimeout
    {
        public int Timeout { get; }
        public BatchReceiveWithTimeout(int timeout)
        {
            Timeout = timeout;
        }
    }
    public sealed class ReceiveResponse
    {
        /// <summary>
        /// Message can be either IMessage<T> or Failure which will contain PulsarExceptions
        /// Need to check for both at the client side
        /// </summary>
        public object Message { get; }
        public ReceiveResponse(object messsage)
        {
            Message = messsage;
        }
    }
    public sealed class BatchReceiveResponse
    {
        /// <summary>
        /// Message can be either IMessages<T> or Failure which will contain PulsarExceptions
        /// Need to check for both at the client side
        /// </summary>
        public object Message { get; }
        public BatchReceiveResponse(object messsage)
        {
            Message = messsage;
        }
    }
}
