using System;

namespace SharpPulsar.Protocol.Proto
{
	public partial class KeyValue
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		

		public sealed class Builder
        {
            private readonly KeyValue _value;

            public Builder()
            {
                _value = new KeyValue();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public KeyValue Build()
            {
                return _value;
            }

			
			public string GetKey()
            {
                return _value.Key;
            }
			public Builder SetKey(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _value.Key = value;
				return this;
			}
			
			public string GetValue()
            {
                return _value.Value;
            }
			public Builder SetValue(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _value.Value = value;
				return this;
			}
			
		}

	}

}
