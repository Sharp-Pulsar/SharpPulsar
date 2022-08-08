using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class KeyLongValue
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly KeyLongValue _long;

            public Builder()
            {
                _long = new KeyLongValue();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public KeyLongValue Build()
            {
                return _long;
            }

			
			
			public string GetKey()
            {
                return _long.Key;
            }
			public Builder SetKey(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _long.Key = value;
				return this;
			}
			
            public Builder SetValue(long value)
            {
                _long.Value = (ulong)value;
				return this;
			}
			
		}

	}

}
