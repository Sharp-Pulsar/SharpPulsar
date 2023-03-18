using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages.Transaction;
using static SharpPulsar.Exceptions.TransactionCoordinatorClientException;

namespace SharpPulsar.TransactionImpl
{
    public sealed class Transaction : ITransaction
    {
        private readonly IActorRef _txn;
        private readonly long _mostSigBits;
        private readonly long _leastSigBits;
        public Transaction(long leastBits, long mostBits, IActorRef txn)
        {
            _mostSigBits = mostBits;
            _leastSigBits = leastBits;
            _txn = txn;
        }

        public void Abort()
        {
            AbortAsync().GetAwaiter().GetResult();
        }
        public async ValueTask AbortAsync()
        {
            var error = await _txn.Ask<TransactionCoordinatorClientException>(Messages.Transaction.Abort.Instance).ConfigureAwait(false);
            if (!(error is NoException))
            {
                _txn.Tell(PoisonPill.Instance);
                throw error;
            }
            _txn.Tell(PoisonPill.Instance);
        }

        public void Commit()
        {
            CommitAsync().GetAwaiter().GetResult();
        }
        public async ValueTask CommitAsync()
        {
            var error = await _txn.Ask<TransactionCoordinatorClientException>(Messages.Transaction.Commit.Instance).ConfigureAwait(false);
            if (!(error is NoException))
            {
                _txn.Tell(PoisonPill.Instance);
                throw error;
            }
            _txn.Tell(PoisonPill.Instance);
        }
        public long LeastSigBits => _leastSigBits;
        public long MostSigBits => _mostSigBits;
        internal IActorRef Txn => _txn;
        public TxnID TxnID => new TxnID(_mostSigBits, _leastSigBits);

        public TransactionState State
        {
            get
            {
                return _txn.Ask<TransactionState>(TransState.Instance).GetAwaiter().GetResult();
            }
        }
    }
}
