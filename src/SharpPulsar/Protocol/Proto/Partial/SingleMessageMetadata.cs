using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
    public partial class SingleMessageMetadata
	{ 
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public sealed class Builder
		{
            readonly SingleMessageMetadata _single;

            public Builder()
            {
                    _single = new SingleMessageMetadata();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public SingleMessageMetadata Build()
            {
                return _single;
            }

			
            public KeyValue GetProperties(int index)
			{
				return _single.Properties[index];
			}
			public Builder SetProperties(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _single.Properties[index] = value;
                return this;
			}
			
			public Builder AddProperties(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _single.Properties.Add(value);

				return this;
			}
			public Builder AddProperties(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _single.Properties.Insert(index, value);

				return this;
			}
			
			
			public Builder AddAllProperties(IEnumerable<KeyValue> values)
			{
				values.ToList().ForEach(_single.Properties.Add);

				return this;
			}
			
			public string GetPartitionKey()
			{
				return _single.PartitionKey;
			}
			public Builder SetPartitionKey(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _single.PartitionKey = value;

				return this;
			}
			
            public Builder SetPayloadSize(int value)
            {
                _single.PayloadSize = value;
				return this;
			}
			
			public bool HasCompactedOut()
			{
				return _single.CompactedOut;
			}
			
            public Builder SetCompactedOut(bool value)
            {
                _single.CompactedOut = value;
				return this;
			}
			
            public Builder SetEventTime(long value)
            {
                _single.EventTime = (ulong) value;
				return this;
			}
			
            public Builder SetPartitionKeyB64Encoded(bool value)
            {
                _single.PartitionKeyB64Encoded = value;
				return this;
			}
			
			public Builder SetOrderingKey(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_single.OrderingKey = value;

				return this;
			}
			
            public Builder SetSequenceId(long value)
            {
                _single.SequenceId = (ulong) value;
				return this;
			}
			
		}

		
		
	}


}
