using System;

namespace SharpPulsar.Protocol.Proto
{
	public partial class AuthData
	{
        public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private AuthData _auth;

            public Builder()
            {
                    _auth = new AuthData();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			

            public AuthData Build()
            {
                return _auth;
            }

			public string GetAuthMethodName()
            {
                return _auth.AuthMethodName;
            }
			public Builder SetAuthMethodName(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _auth.AuthMethodName = value;
				return this;
			}
			
            public Builder SetAuthData(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _auth.auth_data = value;
				return this;
			}
			
		}

	}

}
