
using System;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class EncryptionKeys
	{
		
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private EncryptionKeys _keys;

            public Builder()
            {
                _keys = new EncryptionKeys();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public EncryptionKeys Build()
            {
                return _keys;
            }

			
			public string GetKey()
            {
                return _keys.Key;
            }
			public Builder SetKey(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _keys.Key = value;
				return this;
			}
			
            public Builder SetValue(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _keys.Value = value;
				return this;
			}
			
            public KeyValue GetMetadata(int index)
			{
				return _keys.Metadatas[index];
			}
			public Builder SetMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _keys.Metadatas[index] = value;

				return this;
			}
			public Builder SetMetadata(int index, KeyValue.Builder builderForValue)
			{
                _keys.Metadatas[index] = builderForValue.Build();

				return this;
			}
			public Builder AddMetadata(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _keys.Metadatas.Add(value);

				return this;
			}
			public Builder AddMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _keys.Metadatas.Insert(index, value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder builderForValue)
			{
                _keys.Metadatas.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int index, KeyValue.Builder builderForValue)
			{
                _keys.Metadatas.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> values)
			{
                _keys.Metadatas.AddRange(values);
				return this;
			}
			

		}

	}

}
