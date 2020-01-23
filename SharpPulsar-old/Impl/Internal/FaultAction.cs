namespace SharpPulsar.Impl.Internal
{
    public enum FaultAction : byte
    {
        Retry,
        Relookup,
        Fault
    }
}
