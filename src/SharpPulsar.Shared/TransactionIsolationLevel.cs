
namespace SharpPulsar.Shared
{
    public enum TransactionIsolationLevel
    {
        // Consumer can only consume all transactional messages which have been committed.
        READ_COMMITTED,
        // Consumer can consume all messages, even transactional messages which have been aborted.
        READ_UNCOMMITTED
    }

}
