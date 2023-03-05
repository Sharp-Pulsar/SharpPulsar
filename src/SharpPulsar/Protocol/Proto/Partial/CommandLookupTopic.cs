
using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandLookupTopic
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandLookupTopic _command;

            public Builder()
            {
                _command = new CommandLookupTopic();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandLookupTopic Build()
            {
                return _command;
            }
						
			
			public string GetTopic()
            {
                return _command.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _command.Topic = value;

				return this;
			}
			
			
            public Builder SetRequestId(long value)
            {
                _command.RequestId = (ulong) value;
				return this;
			}
			
            public Builder SetAuthoritative(bool value)
            {
                _command.Authoritative = value;
				return this;
			}
			
			public string GetOriginalPrincipal()
            {
                return _command.OriginalPrincipal;
            }
			public Builder SetOriginalPrincipal(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _command.OriginalPrincipal = value;
				return this;
			}
			
			public string GetOriginalAuthData()
            {
                return _command.OriginalAuthData;
            }
			public Builder SetOriginalAuthData(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _command.OriginalAuthData = value;

				return this;
			}
			
			public string GetOriginalAuthMethod()
            {
                return _command.OriginalAuthMethod;
            }
			public Builder SetOriginalAuthMethod(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _command.OriginalAuthMethod = value;

				return this;
			}
			
		}

	}

}
