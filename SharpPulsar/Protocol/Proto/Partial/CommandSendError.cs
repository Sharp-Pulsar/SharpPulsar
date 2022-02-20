using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSendError
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandSendError _error;

            public Builder()
            {
                _error = new CommandSendError();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandSendError Build()
            {
                return _error;
            }

			
            public Builder SetProducerId(long value)
            {
                _error.ProducerId = (ulong)value;
				return this;
			}
			public Builder SetSequenceId(long value)
            {
                _error.SequenceId = (ulong) value;
				return this;
			}
			
            public Builder SetError(ServerError value)
			{
                _error.Error = value;
				return this;
			}
			
			public string GetMessage()
            {
                return _error.Message;
            }
			public Builder SetMessage(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _error.Message = value;
				return this;
			}
			
		}

	}

}
