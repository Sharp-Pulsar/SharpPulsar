namespace SharpPulsar.Impl
{
    public enum SubscriptionMode
    {
        // Make the subscription to be backed by a durable cursor that will retain messages and persist the current
        // position
        Durable,

        // Lightweight subscription mode that doesn't have a durable cursor associated
        NonDurable
    }
}