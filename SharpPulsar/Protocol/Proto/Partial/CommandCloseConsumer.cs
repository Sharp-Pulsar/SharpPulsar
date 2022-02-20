
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandCloseConsumer
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandCloseConsumer _close;

            public Builder()
            {
                _close = new CommandCloseConsumer();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandCloseConsumer Build()
            {
                return _close;
            }

			
            public Builder SetConsumerId(long value)
            {
                _close.ConsumerId = (ulong)value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _close.RequestId = (ulong) value;
				return this;
			}
			
		}

	}

}
