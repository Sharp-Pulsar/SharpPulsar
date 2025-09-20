using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetOrCreateSchema 
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandGetOrCreateSchema _schema;

            public Builder()
            {
                _schema = new CommandGetOrCreateSchema();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandGetOrCreateSchema Build()
            {
                return _schema;
            }
			
            public Builder SetRequestId(long value)
            {
                _schema.RequestId = (ulong) value;
				return this;
			}
			
			public string GetTopic()
            {
                return _schema.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _schema.Topic = value;
				return this;
			}
			
			public Schema GetSchema()
			{
				return _schema.Schema;
			}
			public Builder SetSchema(Schema value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _schema.Schema = value;
				return this;
			}
			public Builder SetSchema(Schema.Builder builderForValue)
			{
                _schema.Schema = builderForValue.Build();
				return this;
			}
			
		}

	}

}
