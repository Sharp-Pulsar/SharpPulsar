using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAck
	{

        public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandAck _ack;

            public Builder()
            {
                _ack = new CommandAck();
            }
			internal static Builder Create()
			{
				return new Builder();
			}


            public CommandAck Build()
            {
                return _ack;
            }

			public bool HasConsumerId()
			{
				return _ack.HasConsumerId;
			}
			
            public Builder SetConsumerId(long value)
            {
                _ack.ConsumerId = (ulong) value;
				return this;
			}
			public bool HasAckType()
			{
				return _ack.HasAckType;
			}
			
            public Builder SetAckType(Types.AckType value)
			{
                _ack.AckType = value;
				return this;
			}
			
            public MessageIdData GetMessageId(int index)
			{
				return _ack.MessageId[index];
			}
			public Builder SetMessageId(int index, MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.MessageId[index] = value;

				return this;
			}
			public Builder SetMessageId(int index, MessageIdData.Builder builderForValue)
			{

                _ack.MessageId[index] = builderForValue.Build();

				return this;
			}
			public Builder AddMessageId(MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.MessageId.Add(value);

				return this;
			}
			public Builder AddMessageId(int index, MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.MessageId.Insert(index, value);

				return this;
			}
			public Builder AddMessageId(MessageIdData.Builder builderForValue)
			{
                _ack.MessageId.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMessageId(int index, MessageIdData.Builder builderForValue)
			{
                _ack.MessageId.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMessageId(IEnumerable<MessageIdData> values)
			{
				values.ToList().ForEach(_ack.MessageId.Add);

				return this;
			}
			public bool HasValidationError()
			{
				return _ack.HasValidationError;
			}
			
            public Builder SetValidationError(Types.ValidationError? value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _ack.ValidationError = (Types.ValidationError) value;
                return this;
            }
			
            public KeyLongValue GetProperties(int index)
			{
				return _ack.Properties[index];
			}
			public Builder SetProperties(int index, KeyLongValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.Properties[index] = value;

				return this;
			}
			public Builder SetProperties(int index, KeyLongValue.Builder builderForValue)
			{
                _ack.Properties[index] = builderForValue.Build();

				return this;
			}
			public Builder AddProperties(KeyLongValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.Properties.Add(value);

				return this;
			}
			public Builder AddProperties(int index, KeyLongValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _ack.Properties.Insert(index, value);

				return this;
			}
			public Builder AddProperties(KeyLongValue.Builder builderForValue)
			{
                _ack.Properties.Add(builderForValue.Build());

				return this;
			}
			public Builder AddProperties(int index, KeyLongValue.Builder builderForValue)
			{
                _ack.Properties.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllProperties(IEnumerable<KeyLongValue> values)
			{
				values.ToList().ForEach(_ack.Properties.Add);

				return this;
			}
			
			public bool HasTxnidLeastBits()
			{
				return _ack.HasTxnidLeastBits;
			}
			
            public Builder SetTxnidLeastBits(long value)
            {
                _ack.TxnidLeastBits = (ulong) value;
				return this;
			}
			
			public bool HasTxnidMostBits()
			{
				return _ack.HasTxnidMostBits;
			}
			
            public Builder SetTxnidMostBits(long value)
            {
                _ack.TxnidMostBits = (ulong) value;
				return this;
			}
			
		}

	}

}
