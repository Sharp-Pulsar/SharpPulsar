using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSeek
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public sealed class Builder
        {
            private CommandSeek _seek;

            public Builder()
            {
                    _seek = new CommandSeek();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			

            public CommandSeek Build()
            {
                return _seek;
            }

			
			public bool HasConsumerId()
			{
				return _seek.HasConsumerId;
			}
			
            public Builder SetConsumerId(long value)
            {
                _seek.ConsumerId = (ulong) value;
				return this;
			}
			
			public bool HasRequestId()
			{
				return _seek.HasRequestId;
			}
			
            public Builder SetRequestId(long value)
            {
                _seek.RequestId = (ulong) value;
				return this;
			}
			
			public bool HasMessageId()
			{
				return _seek.HasMessageId;
			}
			public MessageIdData GetMessageId()
			{
				return _seek.MessageId;
			}
			public Builder SetMessageId(MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _seek.MessageId = value;
				return this;
			}
			public Builder SetMessageId(MessageIdData.Builder builderForValue)
			{
                _seek.MessageId = builderForValue.Build();
				return this;
			}
			
			public bool HasMessagePublishTime()
			{
				return _seek.HasMessagePublishTime;
			}
			
            public Builder SetMessagePublishTime(long value)
            {
                _seek.MessagePublishTime = (ulong) value;
				return this;
			}
			
		}

	}

}
