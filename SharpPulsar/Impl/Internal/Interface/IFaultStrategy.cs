using System;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IFaultStrategy
    {
        FaultAction DetermineFaultAction(System.Exception exception);
        TimeSpan RetryInterval { get; }
    }
}
