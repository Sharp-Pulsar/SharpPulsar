using System.Collections.Concurrent;
using Akka.Actor;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Transaction;

namespace SharpPulsar.User
{
    public sealed class Transaction:ITransaction
    {
        private readonly IActorRef _txn;
        private readonly BlockingCollection<TransactionCoordinatorClientException> _queue;
        public Transaction(IActorRef txn, BlockingCollection<TransactionCoordinatorClientException> queue)
        {
            _txn = txn;
            _queue = queue;
        }

        public void Abort()
        {
            _txn.Tell(Messages.Transaction.Abort.Instance);
            var error = _queue.Take();
            if (error != null)
            {
                _txn.Tell(PoisonPill.Instance);
                throw error;
            }
            _txn.Tell(PoisonPill.Instance);
        }

        public void Commit()
        {
            _txn.Tell(Messages.Transaction.Commit.Instance);
            var error = _queue.Take();
            if (error != null)
            {
                _txn.Tell(PoisonPill.Instance);
                throw error;
            }
            _txn.Tell(PoisonPill.Instance);
        }
        internal IActorRef Txn => _txn;
    }
}
