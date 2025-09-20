
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandReachedEndOfTopic
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandReachedEndOfTopic _command;

            public Builder()
            {
                _command = new CommandReachedEndOfTopic();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandReachedEndOfTopic Build()
            {
                return _command;
            }
			
            public Builder SetConsumerId(long value)
            {
                _command.ConsumerId = (ulong)value;
				return this;
			}
			
		}

	}

}
