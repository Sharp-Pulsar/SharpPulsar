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
			// Construct using org.apache.pulsar.common.api.proto.BaseCommand.newBuilder()
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
                _base.Type = value;
                return this;
			}
			
			public bool HasConnect()
			{
				return _base.HasConnect;
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
			
			public bool HasConnected()
			{
				return _base.HasConnected;
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
			
			public bool HasSubscribe()
			{
				return _base.HasSubscribe;
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
			
			public bool HasProducer()
			{
				return _base.HasProducer;
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
			
			public bool HasSend()
			{
				return _base.HasSend;
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
			
			public bool HasSendReceipt()
			{
				return _base.HasSendReceipt;
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
			
			public bool HasSendError()
			{
				return _base.HasSendError;
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
			public bool HasMessage()
			{
				return _base.HasMessage;
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
			
			
			public bool HasAck()
			{
				return _base.HasAck;
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
			public bool HasFlow()
			{
				return _base.HasFlow;
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
			
			public bool HasUnsubscribe()
			{
				return _base.HasUnsubscribe;
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
			public bool HasRedeliverUnacknowledgedMessages()
			{
				return _base.HasRedeliverUnacknowledgedMessages;
			}
			public CommandRedeliverUnacknowledgedMessages GetRedeliverUnacknowledgedMessages()
			{
				return _base.RedeliverUnacknowledgedMessages;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages value)
			{
                _base.RedeliverUnacknowledgedMessages = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder builderForValue)
			{
                _base.RedeliverUnacknowledgedMessages = builderForValue.Build();
				return this;
			}
			public bool HasPartitionMetadata()
			{
				return _base.HasPartitionMetadata;
			}
			public CommandPartitionedTopicMetadata GetPartitionMetadata()
			{
				return _base.PartitionMetadata;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata value)
			{
                _base.PartitionMetadata = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata.Builder builderForValue)
			{
                _base.PartitionMetadata = builderForValue.Build();
				return this;
			}
			public bool HasLookupTopic()
			{
				return _base.HasLookupTopic;
			}
			public CommandLookupTopic GetLookupTopic()
			{
				return _base.LookupTopic;
			}
			public Builder SetLookupTopic(CommandLookupTopic value)
			{
                _base.LookupTopic = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetLookupTopic(CommandLookupTopic.Builder builderForValue)
			{
                _base.LookupTopic = builderForValue.Build();
				return this;
			}
			
			public bool HasConsumerStats()
			{
				return _base.HasConsumerStats;
			}
			public CommandConsumerStats GetConsumerStats()
			{
				return _base.ConsumerStats;
			}
			public Builder SetConsumerStats(CommandConsumerStats value)
			{
                _base.ConsumerStats = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetConsumerStats(CommandConsumerStats.Builder builderForValue)
			{
                _base.ConsumerStats = builderForValue.Build();
				return this;
			}
			public bool HasReachedEndOfTopic()
			{
				return _base.HasReachedEndOfTopic;
			}
			public CommandReachedEndOfTopic GetReachedEndOfTopic()
			{
				return _base.ReachedEndOfTopic;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic value)
			{
                _base.ReachedEndOfTopic = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic.Builder builderForValue)
			{
                _base.ReachedEndOfTopic = builderForValue.Build();
				return this;
			}
			public bool HasSeek()
			{
				return _base.HasSeek;
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
			public bool HasGetLastMessageId()
			{
				return _base.HasGetLastMessageId;
			}
			public CommandGetLastMessageId GetGetLastMessageId()
			{
				return _base.GetLastMessageId;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId value)
			{
                _base.GetLastMessageId = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId.Builder builderForValue)
			{
                _base.GetLastMessageId = builderForValue.Build();
				return this;
			}
			public bool HasActiveConsumerChange()
			{
				return _base.HasActiveConsumerChange;
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
			public bool HasGetTopicsOfNamespace()
			{
				return _base.HasGetTopicsOfNamespace;
			}
			public CommandGetTopicsOfNamespace GetGetTopicsOfNamespace()
			{
				return _base.GetTopicsOfNamespace;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace value)
			{
                _base.GetTopicsOfNamespace = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder builderForValue)
			{
                _base.GetTopicsOfNamespace = builderForValue.Build();
				return this;
			}
			public bool HasGetSchema()
			{
				return _base.HasGetSchema;
			}
			public CommandGetSchema GetGetSchema()
			{
				return _base.GetSchema;
			}
			public Builder SetGetSchema(CommandGetSchema value)
			{
                _base.GetSchema = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetSchema(CommandGetSchema.Builder builderForValue)
			{
                _base.GetSchema = builderForValue.Build();
				return this;
			}
			public bool HasAuthChallenge()
			{
				return _base.HasAuthChallenge;
			}
			public CommandAuthChallenge GetAuthChallenge()
			{
				return _base.AuthChallenge;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge value)
			{
                _base.AuthChallenge = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge.Builder builderForValue)
			{
                _base.AuthChallenge = builderForValue.Build();
				return this;
			}
			
			public bool HasGetOrCreateSchema()
			{
				return _base.HasGetOrCreateSchema;
			}
			public CommandGetOrCreateSchema GetGetOrCreateSchema()
			{
				return _base.GetOrCreateSchema;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema value)
			{
                _base.GetOrCreateSchema = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema.Builder builderForValue)
			{
                _base.GetOrCreateSchema = builderForValue.Build();
				return this;
			}
			public bool HasNewTxn()
			{
				return _base.HasNewTxn;
			}
			public CommandNewTxn GetNewTxn()
			{
				return _base.NewTxn;
			}
			public Builder SetNewTxn(CommandNewTxn value)
			{
                _base.NewTxn = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetNewTxn(CommandNewTxn.Builder builderForValue)
			{
                _base.NewTxn = builderForValue.Build();
				return this;
			}
			public bool HasAddPartitionToTxn()
			{
				return _base.HasAddPartitionToTxn;
			}
			public CommandAddPartitionToTxn GetAddPartitionToTxn()
			{
				return _base.AddPartitionToTxn;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn value)
			{
                _base.AddPartitionToTxn = value ?? throw new NullReferenceException();

				
				return this;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn.Builder builderForValue)
			{
                _base.AddPartitionToTxn = builderForValue.Build();

				
				return this;
			}
			public bool HasAddSubscriptionToTxn()
			{
				return _base.HasAddSubscriptionToTxn;
			}
			public CommandAddSubscriptionToTxn GetAddSubscriptionToTxn()
			{
				return _base.AddSubscriptionToTxn;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn value)
			{
                _base.AddSubscriptionToTxn = value ?? throw new NullReferenceException();

                return this;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder builderForValue)
			{
                _base.AddSubscriptionToTxn = builderForValue.Build();

                return this;
			}
			
			public bool HasEndTxn()
			{
				return _base.HasEndTxn;
			}
			public CommandEndTxn GetEndTxn()
			{
				return _base.EndTxn;
			}
			public Builder SetEndTxn(CommandEndTxn value)
			{
                _base.EndTxn = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxn(CommandEndTxn.Builder builderForValue)
			{
                _base.EndTxn = builderForValue.Build();
				return this;
			}
			public bool HasEndTxnOnPartition()
			{
				return _base.HasEndTxnOnPartition;
			}
			public CommandEndTxnOnPartition GetEndTxnOnPartition()
			{
				return _base.EndTxnOnPartition;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition value)
			{
                _base.EndTxnOnPartition = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition.Builder builderForValue)
			{
                _base.EndTxnOnPartition = builderForValue.Build();
				return this;
			}
			
			public bool HasEndTxnOnSubscription()
			{
				return _base.HasEndTxnOnSubscription;
			}
			public CommandEndTxnOnSubscription GetEndTxnOnSubscription()
			{
				return _base.EndTxnOnSubscription;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription value)
			{
                _base.EndTxnOnSubscription = value ?? throw new NullReferenceException();
				return this;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder builderForValue)
			{
                _base.EndTxnOnSubscription = builderForValue.Build();
				return this;
			}
			
		}

	}

}
