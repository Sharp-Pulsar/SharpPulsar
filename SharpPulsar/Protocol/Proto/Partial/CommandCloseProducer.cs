
namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandCloseProducer
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandCloseProducer _producer;

            public Builder()
            {
                _producer = new CommandCloseProducer();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandCloseProducer Build()
            {
                return _producer;
            }

            public Builder SetProducerId(long value)
            {
                _producer.ProducerId = (ulong)value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _producer.RequestId = (ulong)value;
				return this;
			}
			
		}

	}

}
