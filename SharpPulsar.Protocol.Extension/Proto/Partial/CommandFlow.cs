
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandFlow
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public sealed class Builder
        {
            private readonly CommandFlow _flow;

            public Builder()
            {
                    _flow = new CommandFlow();
            }
			internal static Builder Create()
			{
				return new Builder();
                
            }

            public CommandFlow Build()
            {
                return _flow;
            }

            public Builder SetConsumerId(long value)
            {
                _flow.ConsumerId = (ulong) value;
				return this;
			}
			
            public Builder SetMessagePermits(int value)
            {
                _flow.messagePermits = (uint) value;
				return this;
			}
			
		}

	}

}
