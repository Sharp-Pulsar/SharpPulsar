using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetSchema 
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandGetSchema _schema;

            public Builder()
            {
                _schema = new CommandGetSchema();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandGetSchema Build()
            {
                return _schema;
            }
			
            public Builder SetRequestId(long value)
            {
                _schema.RequestId = (ulong)value;
				return this;
			}
			
			public string GetTopic()
            {
                return _schema.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _schema.Topic = value;
				return this;
			}
			
            public Builder SetSchemaVersion(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _schema.SchemaVersion = value;
				return this;
			}
			
		}

	}

}
