
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandNewTxn
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandNewTxn _txn;

            public Builder()
            {
                _txn = new CommandNewTxn();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandNewTxn Build()
            {
                return _txn;
            }

			
            public Builder SetRequestId(long value)
            {
                _txn.RequestId = (ulong)value;
				return this;
			}
			
            public Builder SetTxnTtlSeconds(long value)
            {
                _txn.TxnTtlSeconds = (ulong) value;
				return this;
			}
			
            public Builder SetTcId(long value)
            {
                _txn.TcId = (ulong) value;
				return this;
			}
			
		}

		
	}

}
