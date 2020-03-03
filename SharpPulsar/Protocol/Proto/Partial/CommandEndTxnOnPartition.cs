using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandEndTxnOnPartition
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandEndTxnOnPartition _txn;

            public Builder()
            {
                _txn = new CommandEndTxnOnPartition();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandEndTxnOnPartition Build()
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
			
			public string GetTopic()
            {
                return _txn.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _txn.Topic = value;
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
