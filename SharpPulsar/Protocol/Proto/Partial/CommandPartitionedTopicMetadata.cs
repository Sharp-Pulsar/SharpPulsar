using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPartitionedTopicMetadata
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandPartitionedTopicMetadata _metadata;

            public Builder()
            {
                _metadata = new CommandPartitionedTopicMetadata();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandPartitionedTopicMetadata Build()
			{
                return _metadata;
			}
						
			
			public string GetTopic()
            {
                return _metadata.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _metadata.Topic = value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _metadata.RequestId = (ulong) value;
				return this;
			}
			
			public string GetOriginalPrincipal()
            {
                return _metadata.OriginalPrincipal;
            }
			public Builder SetOriginalPrincipal(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _metadata.OriginalPrincipal = value;

				return this;
			}
			
			public string GetOriginalAuthData()
            {
                return _metadata.OriginalAuthData;
            }
			public Builder SetOriginalAuthData(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _metadata.OriginalAuthData = value;

				return this;
			}
			
			public string GetOriginalAuthMethod()
            {
                return _metadata.OriginalAuthMethod;
            }
			public Builder SetOriginalAuthMethod(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _metadata.OriginalAuthMethod = value;

				return this;
			}
			
		}

	}

}
