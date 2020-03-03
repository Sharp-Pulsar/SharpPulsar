
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandEndTxn
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandEndTxn _txn;

            public Builder()
            {
                _txn = new CommandEndTxn();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandEndTxn Build()
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
			
            public Builder SetTxnAction(TxnAction value)
			{
                _txn.TxnAction = value;
				return this;
			}
			

		}

		
	}

}
