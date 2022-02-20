
namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandSend
	{
		// Use CommandSend.newBuilder() to construct.
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
		{
            readonly CommandSend _send = new CommandSend();
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandSend Build()
			{
				
				return _send;
			}
			
			

            public Builder SetProducerId(long value)
            {
                _send.ProducerId = (ulong)value;
				return this;
			}
			
			
            public Builder SetSequenceId(long value)
            {
                _send.SequenceId = (ulong)value;
				return this;
			}
			
            public Builder SetNumMessages(int value)
            {
                _send.NumMessages = value;
				return this;
			}
			
            public Builder SetTxnidLeastBits(long value)
            {
                _send.TxnidLeastBits = (ulong)value;
				return this;
			}
			
            public Builder SetTxnidMostBits(long value)
            {
                _send.TxnidMostBits = (ulong)value;
                return this;
			}
			
            public Builder SetHighestSequenceId(long value)
            {
                _send.HighestSequenceId = (ulong)value;
                return this;
			}
			
		}

	}

}
