
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandActiveConsumerChange
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandActiveConsumerChange _consumer;

            public Builder()
            {
                    _consumer = new CommandActiveConsumerChange();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandActiveConsumerChange Build()
            {
                return _consumer;
            }

			
            public Builder SetConsumerId(long value)
            {
                _consumer.ConsumerId = (ulong)value;
				return this;
			}
			
            public Builder SetIsActive(bool value)
            {
                _consumer.IsActive = value;
				return this;
			}
			
		}

	}

}
