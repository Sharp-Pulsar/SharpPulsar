
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPing
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandPing _ping;

            public Builder()
            {
                _ping = new CommandPing();
                ;
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandPing Build()
            {
                return _ping;
            }

			
		}

		
	}

}
