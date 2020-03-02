using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandSubscribe
	{

        public static Types.InitialPosition ValueOf(int value)
        {
            switch (value)
            {
                case 0:
                    return Types.InitialPosition.Latest;
                case 1:
                    return Types.InitialPosition.Earliest;
                default:
                    return Types.InitialPosition.Latest;
            }
        }
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
		{
			CommandSubscribe _subscribe;

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

			public bool HasTopic()
			{
				return _subscribe.HasTopic;
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
			
			public bool HasSubscription()
			{
				return _subscribe.HasSubscription;
			}
			public string GetSubscription()
            {
                return _subscribe.Subscription;
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
			
			public bool HasSubType()
			{
				return _subscribe.HasSubType;
			}
			
            public Builder SetSubType(Types.SubType value)
			{
                _subscribe.SubType = value;

				return this;
			}
			public bool HasConsumerId()
			{
				return _subscribe.HasConsumerId;
			}
			
            public Builder SetConsumerId(long value)
            {
                _subscribe.ConsumerId = (ulong)value;
				return this;
			}
			
			public bool HasRequestId()
			{
				return _subscribe.HasRequestId;
			}
			
            public Builder SetRequestId(long value)
            {
                _subscribe.RequestId = (ulong) value;
				return this;
			}
			
			public bool HasConsumerName()
			{
				return _subscribe.HasConsumerName;
			}
			public Builder SetConsumerName(string value)
			{
				_subscribe.ConsumerName = value ?? throw new NullReferenceException();

				return this;
			}
			
			public bool HasPriorityLevel()
			{
				return _subscribe.PriorityLevel;
			}
			
            public Builder SetPriorityLevel(int value)
            {
                _subscribe.PriorityLevel = value;
				return this;
			}
			
			public bool HasDurable()
			{
				return _subscribe.HasDurable;
			}
			
            public Builder SetDurable(bool value)
            {
                _subscribe.Durable = value;
				return this;
			}
			
			public bool HasStartMessageId()
			{
				return _subscribe.HasStartMessageId;
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
			
			public static Types.SubType ValueOf(int value)
			{
				switch (value)
				{
					case 0:
						return Types.SubType.Exclusive;
					case 1:
						return Types.SubType.Shared;
					case 2:
						return Types.SubType.Failover;
					case 3:
						return Types.SubType.KeyShared;
					default:
						return Types.SubType.Exclusive;
				}
			}
			
            public KeyValue GetMetadata(int index)
			{
				return _subscribe.Metadata[index];
			}
			public Builder SetMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadata[index] = value;

				return this;
			}
			public Builder SetMetadata(int index, KeyValue.Builder builderForValue)
			{
                _subscribe.Metadata[index] = builderForValue.Build();
                return this;
			}
			public Builder AddMetadata(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadata.Add(value);

				return this;
			}
			public Builder AddMetadata(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _subscribe.Metadata.Insert(index, value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder builderForValue)
			{
                _subscribe.Metadata.Add(builderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int index, KeyValue.Builder builderForValue)
			{
                _subscribe.Metadata.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> values)
			{
				values.ToList().ForEach(_subscribe.Metadata.Add);

				return this;
			}
			public bool HasReadCompacted()
			{
				return _subscribe.HasReadCompacted;
			}
			
            public Builder SetReadCompacted(bool value)
            {
                _subscribe.ReadCompacted = value;
				return this;
			}
			
			public bool HasSchema()
			{
				return _subscribe.HasSchema;
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
			public bool HasInitialPosition()
			{
				return _subscribe.HasInitialPosition;
			}
			
            public Builder SetInitialPosition(Types.InitialPosition value)
			{
                _subscribe.InitialPosition = value;
				return this;
			}
			
			public bool HasReplicateSubscriptionState()
			{
				return _subscribe.HasReplicateSubscriptionState;
			}
			
            public Builder SetReplicateSubscriptionState(bool value)
            {
                _subscribe.ReplicateSubscriptionState = value;
				return this;
			}
			
			public bool HasForceTopicCreation()
			{
				return _subscribe.HasForceTopicCreation;
			}
			
            public Builder SetForceTopicCreation(bool value)
            {
                _subscribe.ForceTopicCreation = value;
				return this;
			}
			
			public bool HasStartMessageRollbackDurationSec()
			{
				return _subscribe.HasStartMessageRollbackDurationSec;
			}
			
            public Builder SetStartMessageRollbackDurationSec(long value)
            {
                _subscribe.StartMessageRollbackDurationSec = (ulong)value;
				return this;
			}
			
			public bool HasKeySharedMeta()
			{
				return _subscribe.HasKeySharedMeta;
			}
			public KeySharedMeta GetKeySharedMeta()
			{
				return _subscribe.KeySharedMeta;
			}
			public Builder SetKeySharedMeta(KeySharedMeta value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _subscribe.KeySharedMeta = value;
				return this;
			}
			public Builder SetKeySharedMeta(KeySharedMeta.Builder builderForValue)
			{
                _subscribe.KeySharedMeta = builderForValue.Build();
				return this;
			}
			

		}

	}

}
