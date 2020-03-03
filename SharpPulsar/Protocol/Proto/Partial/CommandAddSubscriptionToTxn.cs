using System;
using System.Collections.Generic;
using System.Linq;
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAddSubscriptionToTxn 
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandAddSubscriptionToTxn _txn;

            public Builder()
            {
                _txn = new CommandAddSubscriptionToTxn();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandAddSubscriptionToTxn Build()
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
			
            public Subscription GetSubscription(int index)
			{
				return _txn.Subscriptions[index];
			}
			public Builder SetSubscription(int index, Subscription value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _txn.Subscriptions[index] = value;

				return this;
			}
			public Builder SetSubscription(int index, Subscription.Builder builderForValue)
			{
                _txn.Subscriptions[index] = builderForValue.Build();

				return this;
			}
			public Builder AddSubscription(Subscription value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				
                _txn.Subscriptions.Add(value);

				return this;
			}
			public Builder AddSubscription(int index, Subscription value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _txn.Subscriptions.Insert(index, value);

				return this;
			}
			public Builder AddSubscription(Subscription.Builder builderForValue)
			{
                _txn.Subscriptions.Add(builderForValue.Build());

				return this;
			}
			public Builder AddSubscription(int index, Subscription.Builder builderForValue)
			{
                _txn.Subscriptions.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllSubscription(IEnumerable<Subscription> values)
			{
				values.ToList().ForEach(_txn.Subscriptions.Add);

				return this;
			}
			
			
		}

	}

}
