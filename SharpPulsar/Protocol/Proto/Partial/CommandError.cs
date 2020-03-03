using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandError
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandError _error;

            public Builder()
            {
                _error = new CommandError();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandError Build()
            {
                return _error;
            }

			
            public Builder SetRequestId(long value)
            {
                _error.RequestId = (ulong) value;
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
