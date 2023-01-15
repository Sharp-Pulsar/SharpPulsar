namespace SharpPulsar.TransactionImpl
{
    public enum TransactionState
    {

        /// <summary>
        /// When a transaction is in the `OPEN` state, messages can be produced and acked with this transaction.
        /// 
        /// When a transaction is in the `OPEN` state, it can commit or abort.
        /// </summary>
        OPEN,

        /// <summary>
        /// When a client invokes a commit, the transaction state is changed from `OPEN` to `COMMITTING`.
        /// </summary>
        COMMITTING,

        /// <summary>
        /// When a client invokes an abort, the transaction state is changed from `OPEN` to `ABORTING`.
        /// </summary>
        ABORTING,

        /// <summary>
        /// When a client receives a response to a commit, the transaction state is changed from
        /// `COMMITTING` to `COMMITTED`.
        /// </summary>
        COMMITTED,

        /// <summary>
        /// When a client receives a response to an abort, the transaction state is changed from `ABORTING` to `ABORTED`.
        /// </summary>
        ABORTED,

        /// <summary>
        /// When a client invokes a commit or an abort, but a transaction does not exist in a coordinator,
        /// then the state is changed to `ERROR`.
        /// 
        /// When a client invokes a commit, but the transaction state in a coordinator is `ABORTED` or `ABORTING`,
        /// then the state is changed to `ERROR`.
        /// 
        /// When a client invokes an abort, but the transaction state in a coordinator is `COMMITTED` or `COMMITTING`,
        /// then the state is changed to `ERROR`.
        /// </summary>
        ERROR,

        /// <summary>
        /// When a transaction is timed out and the transaction state is `OPEN`,
        /// then the transaction state is changed from `OPEN` to `TIME_OUT`.
        /// </summary>
        TimeOut
    }

}
