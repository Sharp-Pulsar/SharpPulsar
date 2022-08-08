
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandConsumerStats
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandConsumerStats _stats;

            public Builder()
            {
                _stats = new CommandConsumerStats();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandConsumerStats Build()
            {
                return _stats;
            }

            public Builder SetRequestId(long value)
            {
                _stats.RequestId = (ulong)value;
				return this;
			}
			
            public Builder SetConsumerId(long value)
            {
                _stats.ConsumerId = (ulong)value;
				return this;
			}
			
		}

	}

}
