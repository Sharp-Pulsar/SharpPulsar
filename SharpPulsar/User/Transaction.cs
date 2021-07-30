using Akka.Actor;
using SharpPulsar.Interfaces.Transaction;

namespace SharpPulsar.User
{
    public sealed class Transaction:ITransaction
    {
        private readonly IActorRef _txn;
        public Transaction(IActorRef txn)
        {
            _txn = txn;
        }

        public void Abort()
        {
            _txn.Tell(Messages.Transaction.Abort.Instance);
        }

        public void Commit()
        {
            _txn.Tell(Messages.Transaction.Commit.Instance);
        }
        internal IActorRef Txn => _txn;
    }
}
