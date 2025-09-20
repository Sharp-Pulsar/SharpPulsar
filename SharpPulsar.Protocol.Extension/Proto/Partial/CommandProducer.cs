using System;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandProducer
	{
		
        public KeyValue GetMetadata(int index)
		{
			return Metadatas[index];
		}
		
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandProducer _producer;

            public Builder()
            {
                _producer = new CommandProducer();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public CommandProducer Build()
            {
               return _producer;
            }

			
			public string GetTopic()
            {
                return _producer.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _producer.Topic = value;

				return this;
			}
			
            public Builder SetProducerId(long value)
            {
                _producer.ProducerId = (ulong)value;
				return this;
			}
			
            public Builder SetRequestId(long value)
			{
				_producer.RequestId = (ulong)value;
				return this;
			}
			
			public string GetProducerName()
            {
                return _producer.ProducerName;
            }
			public Builder SetProducerName(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _producer.ProducerName = value;

				return this;
			}
			
            public Builder SetEncrypted(bool value)
            {
                _producer.Encrypted = value;
				return this;
			}
			
            public KeyValue GetMetadata(int index)
			{
				return _producer.Metadatas[index];
			}
			public Builder SetMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				
                _producer.Metadatas[index] = value;

				return this;
			}
			public Builder SetMetadata(int index, KeyValue.Builder builderForValue)
			{
                _producer.Metadatas[index] = builderForValue.Build();

				return this;
			}
			public Builder AddMetadata(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _producer.Metadatas.Add(value);

				return this;
			}
			public Builder AddMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _producer.Metadatas.Insert(index, value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder builderForValue)
			{
                _producer.Metadatas.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int index, KeyValue.Builder builderForValue)
			{
                _producer.Metadatas.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> values)
			{
				_producer.Metadatas.AddRange(values);

				return this;
			}
			
			public Schema GetSchema()
			{
				return _producer.Schema;
			}
			public Builder SetSchema(Schema value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _producer.Schema = value;
				return this;
			}
			public Builder SetSchema(Schema.Builder builderForValue)
			{
                _producer.Schema = builderForValue.Build();
				return this;
			}
			
            public Builder SetEpoch(long value)
            {
                _producer.Epoch = (ulong)value;
				return this;
			}
			
            public Builder SetUserProvidedProducerName(bool value)
            {
                _producer.UserProvidedProducerName = value;
				return this;
			}
			
		}

	}

}
