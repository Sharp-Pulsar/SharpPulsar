
using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandEndTxnOnSubscription
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandEndTxnOnSubscription _txn;

            public Builder()
            {
                _txn = new CommandEndTxnOnSubscription();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandEndTxnOnSubscription Build()
            {
                return _txn;
            }
			
            public Builder SetRequestId(long value)
            {
                _txn.RequestId = (ulong)value;
				return this;
			}
			
            public Builder SetTxnidLeastBits(long value)
            {
                _txn.TxnidLeastBits = (ulong)value;
				return this;
			}
			
            public Builder SetTxnidMostBits(long value)
            {
                _txn.TxnidMostBits = (ulong)value;
				return this;
			}
			
			public Subscription GetSubscription()
			{
				return _txn.Subscription;
			}
			public Builder SetSubscription(Subscription value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _txn.Subscription = value;
				return this;
			}
			public Builder SetSubscription(Subscription.Builder builderForValue)
			{
				_txn.Subscription = builderForValue.Build();
				return this;
			}
			
            public Builder SetTxnAction(TxnAction value)
			{
                _txn.TxnAction = value;
				return this;
			}
			
		}

	}

}
