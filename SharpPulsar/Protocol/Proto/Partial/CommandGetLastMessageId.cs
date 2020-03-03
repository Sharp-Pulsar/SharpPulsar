
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetLastMessageId
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandGetLastMessageId _last;

            public Builder()
            {
                _last = new CommandGetLastMessageId();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandGetLastMessageId Build()
            {
                return _last;
            }
			
            public Builder SetConsumerId(long value)
            {
                _last.ConsumerId = (ulong) value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _last.RequestId = (ulong) value;
				return this;
			}
			
		}

	}

}
