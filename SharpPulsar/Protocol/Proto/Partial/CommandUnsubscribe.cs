
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandUnsubscribe
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		
		public sealed class Builder
        {
            private CommandUnsubscribe _command;

            public Builder()
            {
                _command = new CommandUnsubscribe();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandUnsubscribe Build()
            {
                return _command;
            }

			
            public Builder SetConsumerId(long value)
            {
                _command.ConsumerId = (ulong) value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _command.RequestId = (ulong) value;
				return this;
			}
			
		}

	}

}
