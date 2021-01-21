using Akka.Actor;
using System;

namespace SharpPulsar.Messages.Requests
{
    public sealed class ReconnectLater
    {
        public Exception Exception { get; }
        public ReconnectLater(Exception exception)
        {
            Exception = exception;
        }
    } 
    
    public sealed class ResetBackoff
    {
        public static ResetBackoff Instance = new ResetBackoff();
    }

    public sealed class GetCnx
    {
        public static GetCnx Instance = new GetCnx();
    }
    public sealed class SetCnx
    {
        public IActorRef ClientCnx { get; }
        public SetCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public sealed class GrabCnx
    {
        public string Message { get; }
        public GrabCnx(string message)
        {
            Message = message;
        }
    }
}
