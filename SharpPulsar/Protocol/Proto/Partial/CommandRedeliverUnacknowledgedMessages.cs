
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandRedeliverUnacknowledgedMessages 
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
		{
            readonly CommandRedeliverUnacknowledgedMessages _messages;

            public Builder()
            {
                _messages = new CommandRedeliverUnacknowledgedMessages();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			public CommandRedeliverUnacknowledgedMessages Build()
            {
                return _messages;
            }
			
            public Builder SetConsumerId(long value)
            {
                _messages.ConsumerId = (ulong) value;
				return this;
			}
			
            public MessageIdData GetMessageIds(int index)
			{
				return _messages.MessageIds[index];
			}
			public Builder SetMessageIds(int index, MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _messages.MessageIds[index] = value;

				return this;
			}
			public Builder SetMessageIds(int index, MessageIdData.Builder builderForValue)
			{
                _messages.MessageIds[index] = builderForValue.Build();

				return this;
			}
			public Builder AddMessageIds(MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _messages.MessageIds.Add(value);

				return this;
			}
			public Builder AddMessageIds(int index, MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _messages.MessageIds.Insert(index, value);

				return this;
			}
			public Builder AddMessageIds(MessageIdData.Builder builderForValue)
			{
                _messages.MessageIds.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMessageIds(int Index, MessageIdData.Builder builderForValue)
			{
                _messages.MessageIds.Insert(Index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMessageIds(IEnumerable<MessageIdData> values)
			{
				values.ToList().ForEach(_messages.MessageIds.Add);

				return this;
			}
			
		}

	}

}
