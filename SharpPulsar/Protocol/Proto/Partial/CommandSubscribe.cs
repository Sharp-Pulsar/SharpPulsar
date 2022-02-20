using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandSubscribe
	{

        public static InitialPosition ValueOf(int value)
        {
            switch (value)
            {
                case 0:
                    return InitialPosition.Latest;
                case 1:
                    return InitialPosition.Earliest;
                default:
                    return InitialPosition.Latest;
            }
        }
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
		{
            readonly CommandSubscribe _subscribe;

            public Builder()
            {
                    _subscribe = new CommandSubscribe();

            }
			internal static Builder Create()
			{
				return new Builder();
			}


            public CommandSubscribe Build()
            {
                return _subscribe;
            }

			public string GetTopic()
            {
                return _subscribe.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _subscribe.Topic = value;
				return this;
			}
			
			public Builder SetSubscription(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _subscribe.Subscription = value;
				return this;
			}
			
            public Builder SetSubType(SubType value)
			{
                _subscribe.subType = value;

				return this;
			}
			
            public Builder SetConsumerId(long value)
            {
                _subscribe.ConsumerId = (ulong)value;
				return this;
			}
			
            public Builder SetRequestId(long value)
            {
                _subscribe.RequestId = (ulong) value;
				return this;
			}
			
			public Builder SetConsumerName(string value)
			{
				_subscribe.ConsumerName = value ?? throw new NullReferenceException();

				return this;
			}

            public Builder SetForceTopicCreation(bool value)
            {
                _subscribe.ForceTopicCreation = value;
                return this;
            }

            public Builder SetKeySharedMeta(KeySharedMeta.Builder value)
            {
                _subscribe.keySharedMeta = value.Build();
                return this;
            }

            public Builder SetStartMessageRollbackDurationSec(long value)
            {
                _subscribe.StartMessageRollbackDurationSec = (ulong) value;
                return this;
            }
			public Builder SetPriorityLevel(int value)
            {
                _subscribe.PriorityLevel = value;
				return this;
			}
			
			
            public Builder SetDurable(bool value)
            {
                _subscribe.Durable = value;
				return this;
			}
			
			public MessageIdData GetStartMessageId()
			{
				return _subscribe.StartMessageId;
			}
			public Builder SetStartMessageId(MessageIdData value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.StartMessageId = value;
				return this;
			}
			public Builder SetStartMessageId(MessageIdData.Builder builderForValue)
			{
                _subscribe.StartMessageId = builderForValue.Build();
				return this;
			}
			
			public static SubType ValueOf(int value)
			{
				switch (value)
				{
					case 0:
						return SubType.Exclusive;
					case 1:
						return SubType.Shared;
					case 2:
						return SubType.Failover;
					case 3:
						return SubType.KeyShared;
					default:
						return SubType.Exclusive;
				}
			}
			
            public KeyValue GetMetadata(int index)
			{
				return _subscribe.Metadatas[index];
			}
			public Builder SetMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadatas[index] = value;

				return this;
			}
			public Builder SetMetadata(int index, KeyValue.Builder builderForValue)
			{
                _subscribe.Metadatas[index] = builderForValue.Build();
                return this;
			}
			public Builder AddMetadata(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadatas.Add(value);

				return this;
			}
			public Builder AddMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadatas.Insert(index, value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder builderForValue)
			{
                _subscribe.Metadatas.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int index, KeyValue.Builder builderForValue)
			{
                _subscribe.Metadatas.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> values)
			{
				values.ToList().ForEach(_subscribe.Metadatas.Add);

				return this;
			}
			
            public Builder SetReadCompacted(bool value)
            {
                _subscribe.ReadCompacted = value;
				return this;
			}
			
			public Schema GetSchema()
			{
				return _subscribe.Schema;
			}
			public Builder SetSchema(Schema value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _subscribe.Schema = value;
				return this;
			}
			public Builder SetSchema(Schema.Builder builderForValue)
			{
				_subscribe.Schema = builderForValue.Build();
				return this;
			}
			
            public Builder SetInitialPosition(InitialPosition value)
			{
                _subscribe.initialPosition = value;
				return this;
			}
			
			
            public Builder SetReplicateSubscriptionState(bool value)
            {
                _subscribe.ReplicateSubscriptionState = value;
				return this;
			}
			
		}

	}

}
