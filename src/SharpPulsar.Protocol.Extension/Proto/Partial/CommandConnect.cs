
namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandConnect
	{
		// Use CommandConnect.newBuilder() to construct.
		
		public static Builder NewBuilder()
		{
			return new Builder();
		}
		
		public sealed class Builder 
		{
            private readonly CommandConnect connect = new CommandConnect();
			public Builder SetClientVersion(string version)
            {
                connect.ClientVersion = version;
				return this;
			}

			public Builder SetAuthMethod(AuthMethod value)
            {
                connect.AuthMethod = value;
                return this;
            }
			
			public Builder SetAuthMethodName(string value)
            {
                connect.AuthMethodName = value;
				return this;
			}
			
			public Builder SetAuthData(byte[] value)
            {
                connect.AuthData = value;
				return this;
			}
			
			public Builder SetProtocolVersion(int value)
            {
                connect.ProtocolVersion = value;
                return this;
			}
            public Builder SetProxyToBrokerUrl(string value)
            {
                connect.ProxyToBrokerUrl = value;
                return this;
            }
            public Builder SetProxyVersion(string value)
            {
                connect.ProxyVersion = value;
                return this;
			}
			
			public Builder SetOriginalPrincipal(string value)
            {
                connect.OriginalPrincipal = value;
				return this;
			}
			
			public Builder SetOriginalAuthData(string value)
            {
                connect.OriginalAuthData = value;
				return this;
			}
			
			public Builder SetOriginalAuthMethod(string value)
            {
                connect.OriginalAuthMethod = value;
				return this;
			}

            public CommandConnect Build()
            {
                return connect;
            }
        }

	}


}
