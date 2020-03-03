using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAuthChallenge
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandAuthChallenge _auth;

            public Builder()
            {
                _auth = new CommandAuthChallenge();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandAuthChallenge Build()
            {
                return _auth;
            }

			
			public string GetServerVersion()
            {
                return _auth.ServerVersion;
            }
			public Builder SetServerVersion(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _auth.ServerVersion = value;
				return this;
			}
			
			public AuthData GetChallenge()
			{
				return _auth.Challenge;
			}
			public Builder SetChallenge(AuthData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_auth.Challenge = value;
				return this;
			}
			public Builder SetChallenge(AuthData.Builder builderForValue)
			{
				_auth.Challenge = builderForValue.Build();
				return this;
			}
			
            public Builder SetProtocolVersion(int value)
            {
                _auth.ProtocolVersion = value;
				return this;
			}
			
		}

	}

}
