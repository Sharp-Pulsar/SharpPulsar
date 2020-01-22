﻿using SharpPulsar.Impl.Internal.Interface;
using System;
using System.Net.Sockets;
using static SharpPulsar.Exception.PulsarClientException;

namespace SharpPulsar.Impl.Internal
{
    public sealed class FaultStrategy : IFaultStrategy
    {
        public FaultStrategy(TimeSpan retryInterval)
        {
            RetryInterval = retryInterval;
        }

        public TimeSpan RetryInterval { get; }

        public FaultAction DetermineFaultAction(Exception exception)
        {
            switch (exception)
            {
                case TooManyRequestsException _: return FaultAction.Retry;
                case StreamNotReadyException _: return FaultAction.Relookup;
                case ServiceNotReadyException _: return FaultAction.Relookup;
                case DotPulsarException _: return FaultAction.Fault;
                case SocketException socketException:
                    switch (socketException.SocketErrorCode)
                    {
                        case SocketError.HostNotFound:
                        case SocketError.HostUnreachable:
                        case SocketError.NetworkUnreachable:
                            return FaultAction.Fault;
                    }
                    break;
            }

            return FaultAction.Relookup;
        }
    }
}
