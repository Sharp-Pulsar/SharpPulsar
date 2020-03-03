using System;

namespace SharpPulsar.Protocol.Proto
{
	public partial class BaseCommand
	{
        public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		

		public  class Builder
		{
            private BaseCommand _base;

            public Builder()
            {
                _base = new BaseCommand();
            }
			public static Builder Create()
			{
				return new Builder();
			}

			public BaseCommand Build()
            {
                return _base;
            }
			
			public Builder SetType(Type value)
            {
                _base.type = value;
                return this;
			}
			

			public CommandConnect GetConnect()
			{
				return _base.Connect;
			}
			public Builder SetConnect(CommandConnect value)
			{
                _base.Connect = value ?? throw new NullReferenceException();
				return this;
			}
            public Builder SetCloseProducer(CommandCloseProducer value)
            {
                _base.CloseProducer = value ?? throw new NullReferenceException();
                return this;
            }
            public Builder SetCloseProducer(CommandCloseProducer.Builder builderForValue)
            {
                _base.CloseProducer = builderForValue.Build();
                return this;
            }
            public Builder SetPong(CommandPong value)
            {
                _base.Pong = value ?? throw new NullReferenceException();
                return this;
            }
            public Builder SetPong(CommandPong.Builder builderForValue)
            {
                _base.Pong = builderForValue.Build();
                return this;
            }
			public Builder SetConnect(CommandConnect.Builder builder)
			{
				_base.Connect = builder.Build();
				return this;
			}
			
			public CommandConnected GetConnected()
			{
				return _base.Connected;
			}
			public Builder SetConnected(CommandConnected value)
			{
                _base.Connected = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetConnected(CommandConnected.Builder builderForValue)
			{
                _base.Connected = builderForValue.Build();
				return this;
			}
			
			public CommandSubscribe GetSubscribe()
			{
				return _base.Subscribe;
			}
			public Builder SetSubscribe(CommandSubscribe value)
			{
                _base.Subscribe = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetSubscribe(CommandSubscribe.Builder builderForValue)
			{
                _base.Subscribe = builderForValue.Build();
				return this;
			}
			
			public CommandProducer GetProducer()
			{
				return _base.Producer;
			}
			public Builder SetProducer(CommandProducer value)
			{
                _base.Producer = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetProducer(CommandProducer.Builder builderForValue)
			{
                _base.Producer = builderForValue.Build();
				return this;
			}
			
			public CommandSend GetSend()
			{
				return _base.Send;
			}
			public Builder SetSend(CommandSend value)
			{
                _base.Send = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetSend(CommandSend.Builder builderForValue)
			{
                _base.Send = builderForValue.Build();
				return this;
			}
			
			public CommandSendReceipt GetSendReceipt()
			{
				return _base.SendReceipt;
			}
			public Builder SetSendReceipt(CommandSendReceipt value)
			{
                _base.SendReceipt = value ?? throw new NullReferenceException();
				return this;
			}
			
			public CommandSendError GetSendError()
			{
				return _base.SendError;
			}
			public Builder SetSendError(CommandSendError value)
			{
                _base.SendError = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetSendError(CommandSendError.Builder builderForValue)
			{
                _base.SendError = builderForValue.Build();
				return this;
			}
			
			public CommandMessage GetMessage()
			{
				return _base.Message;
			}
			public Builder SetMessage(CommandMessage value)
			{
                _base.Message = value ?? throw new NullReferenceException();
				return this;
			}
			
			public CommandAck GetAck()
			{
				return _base.Ack;
			}
			public Builder SetAck(CommandAck value)
			{
                _base.Ack = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetAck(CommandAck.Builder builderForValue)
			{
                _base.Ack = builderForValue.Build();
				return this;
			}
			
            public Builder SetCloseConsumer(CommandCloseConsumer value)
            {
                _base.CloseConsumer = value ?? throw new NullReferenceException();
                return this;
            }
            public Builder SetCloseConsumer(CommandCloseConsumer.Builder builderForValue)
            {
                _base.CloseConsumer = builderForValue.Build();
                return this;
            }
			public CommandFlow GetFlow()
			{
				return _base.Flow;
			}
			public Builder SetFlow(CommandFlow value)
			{
                _base.Flow = value ?? throw new NullReferenceException();
                
				return this;
			}
			public Builder SetFlow(CommandFlow.Builder builderForValue)
			{
                _base.Flow = builderForValue.Build();

                return this;
			}
			
			public CommandUnsubscribe GetUnsubscribe()
			{
				return _base.Unsubscribe;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe value)
			{
                _base.Unsubscribe = value ?? throw new NullReferenceException();

                return this;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe.Builder builderForValue)
			{
                _base.Unsubscribe = builderForValue.Build();

				
				return this;
			}
			
			public CommandPing GetPing()
			{
				return _base.Ping;
			}
			public Builder SetPing(CommandPing value)
			{
                _base.Ping = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetPing(CommandPing.Builder builderForValue)
			{
                _base.Ping = builderForValue.Build();
				return this;
			}
			
			public CommandRedeliverUnacknowledgedMessages GetRedeliverUnacknowledgedMessages()
			{
				return _base.redeliverUnacknowledgedMessages;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages value)
			{
                _base.redeliverUnacknowledgedMessages = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder builderForValue)
			{
                _base.redeliverUnacknowledgedMessages = builderForValue.Build();
				return this;
			}
			
			public CommandPartitionedTopicMetadata GetPartitionMetadata()
			{
				return _base.partitionMetadata;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata value)
			{
                _base.partitionMetadata = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata.Builder builderForValue)
			{
                _base.partitionMetadata = builderForValue.Build();
				return this;
			}
			
			public CommandLookupTopic GetLookupTopic()
			{
				return _base.lookupTopic;
			}
			public Builder SetLookupTopic(CommandLookupTopic value)
			{
                _base.lookupTopic = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetLookupTopic(CommandLookupTopic.Builder builderForValue)
			{
                _base.lookupTopic = builderForValue.Build();
				return this;
			}
			
			public CommandConsumerStats GetConsumerStats()
			{
				return _base.consumerStats;
			}
			public Builder SetConsumerStats(CommandConsumerStats value)
			{
                _base.consumerStats = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetConsumerStats(CommandConsumerStats.Builder builderForValue)
			{
                _base.consumerStats = builderForValue.Build();
				return this;
			}
			
			public CommandReachedEndOfTopic GetReachedEndOfTopic()
			{
				return _base.reachedEndOfTopic;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic value)
			{
                _base.reachedEndOfTopic = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic.Builder builderForValue)
			{
                _base.reachedEndOfTopic = builderForValue.Build();
				return this;
			}
			
			public CommandSeek GetSeek()
			{
				return _base.Seek;
			}
			public Builder SetSeek(CommandSeek value)
			{
                _base.Seek = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetSeek(CommandSeek.Builder builderForValue)
			{
                _base.Seek = builderForValue.Build();
				return this;
			}
			public CommandGetLastMessageId GetGetLastMessageId()
			{
				return _base.getLastMessageId;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId value)
			{
                _base.getLastMessageId = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId.Builder builderForValue)
			{
                _base.getLastMessageId = builderForValue.Build();
				return this;
			}
			
			public CommandActiveConsumerChange GetActiveConsumerChange()
			{
				return _base.ActiveConsumerChange;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange value)
			{
                _base.ActiveConsumerChange = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange.Builder builderForValue)
			{
                _base.ActiveConsumerChange = builderForValue.Build();
				return this;
			}
			
			public CommandGetTopicsOfNamespace GetGetTopicsOfNamespace()
			{
				return _base.getTopicsOfNamespace;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace value)
			{
                _base.getTopicsOfNamespace = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder builderForValue)
			{
                _base.getTopicsOfNamespace = builderForValue.Build();
				return this;
			}
			
			public CommandGetSchema GetGetSchema()
			{
				return _base.getSchema;
			}
			public Builder SetGetSchema(CommandGetSchema value)
			{
                _base.getSchema = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetSchema(CommandGetSchema.Builder builderForValue)
			{
                _base.getSchema = builderForValue.Build();
				return this;
			}
			
			public CommandAuthChallenge GetAuthChallenge()
			{
				return _base.authChallenge;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge value)
			{
                _base.authChallenge = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge.Builder builderForValue)
			{
                _base.authChallenge = builderForValue.Build();
				return this;
			}
			
			public CommandGetOrCreateSchema GetGetOrCreateSchema()
			{
				return _base.getOrCreateSchema;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema value)
			{
                _base.getOrCreateSchema = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema.Builder builderForValue)
			{
                _base.getOrCreateSchema = builderForValue.Build();
				return this;
			}
			
			public CommandNewTxn GetNewTxn()
			{
				return _base.newTxn;
			}
			public Builder SetNewTxn(CommandNewTxn value)
			{
                _base.newTxn = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetNewTxn(CommandNewTxn.Builder builderForValue)
			{
                _base.newTxn = builderForValue.Build();
				return this;
			}
			
			public CommandAddPartitionToTxn GetAddPartitionToTxn()
			{
				return _base.addPartitionToTxn;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn value)
			{
                _base.addPartitionToTxn = value ?? throw new NullReferenceException();

				
				return this;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn.Builder builderForValue)
			{
                _base.addPartitionToTxn = builderForValue.Build();

				
				return this;
			}
			
			public CommandAddSubscriptionToTxn GetAddSubscriptionToTxn()
			{
				return _base.addSubscriptionToTxn;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn value)
			{
                _base.addSubscriptionToTxn = value ?? throw new NullReferenceException();

                return this;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder builderForValue)
			{
                _base.addSubscriptionToTxn = builderForValue.Build();

                return this;
			}
			
			public CommandEndTxn GetEndTxn()
			{
				return _base.endTxn;
			}
			public Builder SetEndTxn(CommandEndTxn value)
			{
                _base.endTxn = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxn(CommandEndTxn.Builder builderForValue)
			{
                _base.endTxn = builderForValue.Build();
				return this;
			}
			
			public CommandEndTxnOnPartition GetEndTxnOnPartition()
			{
				return _base.endTxnOnPartition;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition value)
			{
                _base.endTxnOnPartition = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition.Builder builderForValue)
			{
                _base.endTxnOnPartition = builderForValue.Build();
				return this;
			}
			
			
			public CommandEndTxnOnSubscription GetEndTxnOnSubscription()
			{
				return _base.endTxnOnSubscription;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription value)
			{
                _base.endTxnOnSubscription = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder builderForValue)
			{
                _base.endTxnOnSubscription = builderForValue.Build();
				return this;
			}
			
		}

	}

}
