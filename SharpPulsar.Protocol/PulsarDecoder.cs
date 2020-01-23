/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Protocol
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;

	using PulsarApi = SharpPulsar.Api.Proto.PulsarApi;
	using BaseCommand = SharpPulsar.Api.Proto.PulsarApi.BaseCommand;
	using CommandAck = SharpPulsar.Api.Proto.PulsarApi.CommandAck;
	using CommandActiveConsumerChange = SharpPulsar.Api.Proto.PulsarApi.CommandActiveConsumerChange;
	using CommandAddPartitionToTxn = SharpPulsar.Api.Proto.PulsarApi.CommandAddPartitionToTxn;
	using CommandAddPartitionToTxnResponse = SharpPulsar.Api.Proto.PulsarApi.CommandAddPartitionToTxnResponse;
	using CommandAddSubscriptionToTxn = SharpPulsar.Api.Proto.PulsarApi.CommandAddSubscriptionToTxn;
	using CommandAddSubscriptionToTxnResponse = SharpPulsar.Api.Proto.PulsarApi.CommandAddSubscriptionToTxnResponse;
	using CommandAuthChallenge = SharpPulsar.Api.Proto.PulsarApi.CommandAuthChallenge;
	using CommandAuthResponse = SharpPulsar.Api.Proto.PulsarApi.CommandAuthResponse;
	using CommandCloseConsumer = SharpPulsar.Api.Proto.PulsarApi.CommandCloseConsumer;
	using CommandCloseProducer = SharpPulsar.Api.Proto.PulsarApi.CommandCloseProducer;
	using CommandConnect = SharpPulsar.Api.Proto.PulsarApi.CommandConnect;
	using CommandConnected = SharpPulsar.Api.Proto.PulsarApi.CommandConnected;
	using CommandConsumerStats = SharpPulsar.Api.Proto.PulsarApi.CommandConsumerStats;
	using CommandConsumerStatsResponse = SharpPulsar.Api.Proto.PulsarApi.CommandConsumerStatsResponse;
	using CommandEndTxn = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxn;
	using CommandEndTxnOnPartition = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxnOnPartition;
	using CommandEndTxnOnPartitionResponse = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxnOnPartitionResponse;
	using CommandEndTxnOnSubscription = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxnOnSubscription;
	using CommandEndTxnOnSubscriptionResponse = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxnOnSubscriptionResponse;
	using CommandEndTxnResponse = SharpPulsar.Api.Proto.PulsarApi.CommandEndTxnResponse;
	using CommandError = SharpPulsar.Api.Proto.PulsarApi.CommandError;
	using CommandFlow = SharpPulsar.Api.Proto.PulsarApi.CommandFlow;
	using CommandGetOrCreateSchema = SharpPulsar.Api.Proto.PulsarApi.CommandGetOrCreateSchema;
	using CommandGetOrCreateSchemaResponse = SharpPulsar.Api.Proto.PulsarApi.CommandGetOrCreateSchemaResponse;
	using CommandGetSchema = SharpPulsar.Api.Proto.PulsarApi.CommandGetSchema;
	using CommandGetSchemaResponse = SharpPulsar.Api.Proto.PulsarApi.CommandGetSchemaResponse;
	using CommandGetTopicsOfNamespace = SharpPulsar.Api.Proto.PulsarApi.CommandGetTopicsOfNamespace;
	using CommandGetTopicsOfNamespaceResponse = SharpPulsar.Api.Proto.PulsarApi.CommandGetTopicsOfNamespaceResponse;
	using CommandLookupTopic = SharpPulsar.Api.Proto.PulsarApi.CommandLookupTopic;
	using CommandLookupTopicResponse = SharpPulsar.Api.Proto.PulsarApi.CommandLookupTopicResponse;
	using CommandMessage = SharpPulsar.Api.Proto.PulsarApi.CommandMessage;
	using CommandNewTxn = SharpPulsar.Api.Proto.PulsarApi.CommandNewTxn;
	using CommandNewTxnResponse = SharpPulsar.Api.Proto.PulsarApi.CommandNewTxnResponse;
	using CommandPartitionedTopicMetadata = SharpPulsar.Api.Proto.PulsarApi.CommandPartitionedTopicMetadata;
	using CommandPartitionedTopicMetadataResponse = SharpPulsar.Api.Proto.PulsarApi.CommandPartitionedTopicMetadataResponse;
	using CommandPing = SharpPulsar.Api.Proto.PulsarApi.CommandPing;
	using CommandPong = SharpPulsar.Api.Proto.PulsarApi.CommandPong;
	using CommandProducer = SharpPulsar.Api.Proto.PulsarApi.CommandProducer;
	using CommandProducerSuccess = SharpPulsar.Api.Proto.PulsarApi.CommandProducerSuccess;
	using CommandReachedEndOfTopic = SharpPulsar.Api.Proto.PulsarApi.CommandReachedEndOfTopic;
	using CommandRedeliverUnacknowledgedMessages = SharpPulsar.Api.Proto.PulsarApi.CommandRedeliverUnacknowledgedMessages;
	using CommandSeek = SharpPulsar.Api.Proto.PulsarApi.CommandSeek;
	using CommandSend = SharpPulsar.Api.Proto.PulsarApi.CommandSend;
	using CommandSendError = SharpPulsar.Api.Proto.PulsarApi.CommandSendError;
	using CommandSendReceipt = SharpPulsar.Api.Proto.PulsarApi.CommandSendReceipt;
	using CommandSubscribe = SharpPulsar.Api.Proto.PulsarApi.CommandSubscribe;
	using CommandSuccess = SharpPulsar.Api.Proto.PulsarApi.CommandSuccess;
	using CommandUnsubscribe = SharpPulsar.Api.Proto.PulsarApi.CommandUnsubscribe;
	using ByteBufCodedInputStream = SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// Basic implementation of the channel handler to process inbound Pulsar data.
	/// </summary>
	public abstract class PulsarDecoder : ChannelInboundHandlerAdapter
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void channelRead(io.netty.channel.ChannelHandlerContext ctx, Object msg) throws Exception
		public override void ChannelRead(ChannelHandlerContext Ctx, object Msg)
		{
			// Get a buffer that contains the full frame
			ByteBuf Buffer = (ByteBuf) Msg;
			PulsarApi.BaseCommand Cmd = null;
			PulsarApi.BaseCommand.Builder CmdBuilder = null;

			try
			{
				// De-serialize the command
				int CmdSize = (int) Buffer.readUnsignedInt();
				int WriterIndex = Buffer.writerIndex();
				Buffer.writerIndex(Buffer.readerIndex() + CmdSize);
				ByteBufCodedInputStream CmdInputStream = ByteBufCodedInputStream.get(Buffer);
				CmdBuilder = PulsarApi.BaseCommand.newBuilder();
				Cmd = CmdBuilder.mergeFrom(CmdInputStream, null).build();
				Buffer.writerIndex(WriterIndex);

				CmdInputStream.recycle();

				if (log.DebugEnabled)
				{
					log.debug("[{}] Received cmd {}", Ctx.channel().remoteAddress(), Cmd.getType());
				}

				MessageReceived();

				switch (Cmd.getType())
				{
				case PARTITIONED_METADATA:
					checkArgument(Cmd.hasPartitionMetadata());
					HandlePartitionMetadataRequest(Cmd.PartitionMetadata);
					Cmd.PartitionMetadata.recycle();
					break;

				case PARTITIONED_METADATA_RESPONSE:
					checkArgument(Cmd.hasPartitionMetadataResponse());
					HandlePartitionResponse(Cmd.PartitionMetadataResponse);
					Cmd.PartitionMetadataResponse.recycle();
					break;

				case LOOKUP:
					checkArgument(Cmd.hasLookupTopic());
					HandleLookup(Cmd.LookupTopic);
					Cmd.LookupTopic.recycle();
					break;

				case LOOKUP_RESPONSE:
					checkArgument(Cmd.hasLookupTopicResponse());
					HandleLookupResponse(Cmd.LookupTopicResponse);
					Cmd.LookupTopicResponse.recycle();
					break;

				case ACK:
					checkArgument(Cmd.hasAck());
					PulsarApi.CommandAck Ack = Cmd.Ack;
					HandleAck(Ack);
					for (int I = 0; I < Ack.MessageIdCount; I++)
					{
						Ack.getMessageId(I).recycle();
					}
					Ack.recycle();
					break;

				case CLOSE_CONSUMER:
					checkArgument(Cmd.hasCloseConsumer());
					HandleCloseConsumer(Cmd.CloseConsumer);
					Cmd.CloseConsumer.recycle();
					break;

				case CLOSE_PRODUCER:
					checkArgument(Cmd.hasCloseProducer());
					HandleCloseProducer(Cmd.CloseProducer);
					Cmd.CloseProducer.recycle();
					break;

				case CONNECT:
					checkArgument(Cmd.hasConnect());
					HandleConnect(Cmd.Connect);
					Cmd.Connect.recycle();
					break;
				case CONNECTED:
					checkArgument(Cmd.hasConnected());
					HandleConnected(Cmd.Connected);
					Cmd.Connected.recycle();
					break;

				case ERROR:
					checkArgument(Cmd.hasError());
					HandleError(Cmd.Error);
					Cmd.Error.recycle();
					break;

				case FLOW:
					checkArgument(Cmd.hasFlow());
					HandleFlow(Cmd.Flow);
					Cmd.Flow.recycle();
					break;

				case MESSAGE:
				{
					checkArgument(Cmd.hasMessage());
					HandleMessage(Cmd.Message, Buffer);
					Cmd.Message.recycle();
					break;
				}
				case PRODUCER:
					checkArgument(Cmd.hasProducer());
					HandleProducer(Cmd.Producer);
					Cmd.Producer.recycle();
					break;

				case SEND:
				{
					checkArgument(Cmd.hasSend());

					// Store a buffer marking the content + headers
					ByteBuf HeadersAndPayload = Buffer.markReaderIndex();
					HandleSend(Cmd.Send, HeadersAndPayload);
					Cmd.Send.recycle();
					break;
				}
				case SEND_ERROR:
					checkArgument(Cmd.hasSendError());
					HandleSendError(Cmd.SendError);
					Cmd.SendError.recycle();
					break;

				case SEND_RECEIPT:
					checkArgument(Cmd.hasSendReceipt());
					HandleSendReceipt(Cmd.SendReceipt);
					Cmd.SendReceipt.recycle();
					break;

				case SUBSCRIBE:
					checkArgument(Cmd.hasSubscribe());
					HandleSubscribe(Cmd.Subscribe);
					Cmd.Subscribe.recycle();
					break;

				case SUCCESS:
					checkArgument(Cmd.hasSuccess());
					HandleSuccess(Cmd.Success);
					Cmd.Success.recycle();
					break;

				case PRODUCER_SUCCESS:
					checkArgument(Cmd.hasProducerSuccess());
					HandleProducerSuccess(Cmd.ProducerSuccess);
					Cmd.ProducerSuccess.recycle();
					break;

				case UNSUBSCRIBE:
					checkArgument(Cmd.hasUnsubscribe());
					HandleUnsubscribe(Cmd.Unsubscribe);
					Cmd.Unsubscribe.recycle();
					break;

				case SEEK:
					checkArgument(Cmd.hasSeek());
					HandleSeek(Cmd.Seek);
					Cmd.Seek.recycle();
					break;

				case PING:
					checkArgument(Cmd.hasPing());
					HandlePing(Cmd.Ping);
					Cmd.Ping.recycle();
					break;

				case PONG:
					checkArgument(Cmd.hasPong());
					HandlePong(Cmd.Pong);
					Cmd.Pong.recycle();
					break;

				case REDELIVER_UNACKNOWLEDGED_MESSAGES:
					checkArgument(Cmd.hasRedeliverUnacknowledgedMessages());
					HandleRedeliverUnacknowledged(Cmd.RedeliverUnacknowledgedMessages);
					Cmd.RedeliverUnacknowledgedMessages.recycle();
					break;

				case CONSUMER_STATS:
					checkArgument(Cmd.hasConsumerStats());
					HandleConsumerStats(Cmd.ConsumerStats);
					Cmd.ConsumerStats.recycle();
					break;

				case CONSUMER_STATS_RESPONSE:
					checkArgument(Cmd.hasConsumerStatsResponse());
					HandleConsumerStatsResponse(Cmd.ConsumerStatsResponse);
					Cmd.ConsumerStatsResponse.recycle();
					break;

				case REACHED_END_OF_TOPIC:
					checkArgument(Cmd.hasReachedEndOfTopic());
					HandleReachedEndOfTopic(Cmd.ReachedEndOfTopic);
					Cmd.ReachedEndOfTopic.recycle();
					break;

				case GET_LAST_MESSAGE_ID:
					checkArgument(Cmd.hasGetLastMessageId());
					HandleGetLastMessageId(Cmd.GetLastMessageId);
					Cmd.GetLastMessageId.recycle();
					break;

				case GET_LAST_MESSAGE_ID_RESPONSE:
					checkArgument(Cmd.hasGetLastMessageIdResponse());
					HandleGetLastMessageIdSuccess(Cmd.GetLastMessageIdResponse);
					Cmd.GetLastMessageIdResponse.recycle();
					break;

				case ACTIVE_CONSUMER_CHANGE:
					HandleActiveConsumerChange(Cmd.ActiveConsumerChange);
					Cmd.ActiveConsumerChange.recycle();
					break;

				case GET_TOPICS_OF_NAMESPACE:
					checkArgument(Cmd.hasGetTopicsOfNamespace());
					HandleGetTopicsOfNamespace(Cmd.GetTopicsOfNamespace);
					Cmd.GetTopicsOfNamespace.recycle();
					break;

				case GET_TOPICS_OF_NAMESPACE_RESPONSE:
					checkArgument(Cmd.hasGetTopicsOfNamespaceResponse());
					HandleGetTopicsOfNamespaceSuccess(Cmd.GetTopicsOfNamespaceResponse);
					Cmd.GetTopicsOfNamespaceResponse.recycle();
					break;

				case GET_SCHEMA:
					checkArgument(Cmd.hasGetSchema());
					HandleGetSchema(Cmd.GetSchema);
					Cmd.GetSchema.recycle();
					break;

				case GET_SCHEMA_RESPONSE:
					checkArgument(Cmd.hasGetSchemaResponse());
					HandleGetSchemaResponse(Cmd.GetSchemaResponse);
					Cmd.GetSchemaResponse.recycle();
					break;

				case GET_OR_CREATE_SCHEMA:
					checkArgument(Cmd.hasGetOrCreateSchema());
					HandleGetOrCreateSchema(Cmd.GetOrCreateSchema);
					Cmd.GetOrCreateSchema.recycle();
					break;

				case GET_OR_CREATE_SCHEMA_RESPONSE:
					checkArgument(Cmd.hasGetOrCreateSchemaResponse());
					HandleGetOrCreateSchemaResponse(Cmd.GetOrCreateSchemaResponse);
					Cmd.GetOrCreateSchemaResponse.recycle();
					break;

				case AUTH_CHALLENGE:
					checkArgument(Cmd.hasAuthChallenge());
					HandleAuthChallenge(Cmd.AuthChallenge);
					Cmd.AuthChallenge.recycle();
					break;

				case AUTH_RESPONSE:
					checkArgument(Cmd.hasAuthResponse());
					HandleAuthResponse(Cmd.AuthResponse);
					Cmd.AuthResponse.recycle();
					break;

				case NEW_TXN:
					checkArgument(Cmd.hasNewTxn());
					HandleNewTxn(Cmd.NewTxn);
					Cmd.NewTxn.recycle();
					break;

				case NEW_TXN_RESPONSE:
					checkArgument(Cmd.hasNewTxnResponse());
					HandleNewTxnResponse(Cmd.NewTxnResponse);
					Cmd.NewTxnResponse.recycle();
					break;

				case ADD_PARTITION_TO_TXN:
					checkArgument(Cmd.hasAddPartitionToTxn());
					HandleAddPartitionToTxn(Cmd.AddPartitionToTxn);
					Cmd.AddPartitionToTxn.recycle();
					break;

				case ADD_PARTITION_TO_TXN_RESPONSE:
					checkArgument(Cmd.hasAddPartitionToTxnResponse());
					HandleAddPartitionToTxnResponse(Cmd.AddPartitionToTxnResponse);
					Cmd.AddPartitionToTxnResponse.recycle();
					break;

				case ADD_SUBSCRIPTION_TO_TXN:
					checkArgument(Cmd.hasAddSubscriptionToTxn());
					HandleAddSubscriptionToTxn(Cmd.AddSubscriptionToTxn);
					Cmd.AddSubscriptionToTxn.recycle();
					break;

				case ADD_SUBSCRIPTION_TO_TXN_RESPONSE:
					checkArgument(Cmd.hasAddSubscriptionToTxnResponse());
					HandleAddSubscriptionToTxnResponse(Cmd.AddSubscriptionToTxnResponse);
					Cmd.AddSubscriptionToTxnResponse.recycle();
					break;

				case END_TXN:
					checkArgument(Cmd.hasEndTxn());
					HandleEndTxn(Cmd.EndTxn);
					Cmd.EndTxn.recycle();
					break;

				case END_TXN_RESPONSE:
					checkArgument(Cmd.hasEndTxnResponse());
					HandleEndTxnResponse(Cmd.EndTxnResponse);
					Cmd.EndTxnResponse.recycle();
					break;

				case END_TXN_ON_PARTITION:
					checkArgument(Cmd.hasEndTxnOnPartition());
					HandleEndTxnOnPartition(Cmd.EndTxnOnPartition);
					Cmd.EndTxnOnPartition.recycle();
					break;

				case END_TXN_ON_PARTITION_RESPONSE:
					checkArgument(Cmd.hasEndTxnOnPartitionResponse());
					HandleEndTxnOnPartitionResponse(Cmd.EndTxnOnPartitionResponse);
					Cmd.EndTxnOnPartitionResponse.recycle();
					break;

				case END_TXN_ON_SUBSCRIPTION:
					checkArgument(Cmd.hasEndTxnOnSubscription());
					HandleEndTxnOnSubscription(Cmd.EndTxnOnSubscription);
					Cmd.EndTxnOnSubscription.recycle();
					break;

				case END_TXN_ON_SUBSCRIPTION_RESPONSE:
					checkArgument(Cmd.hasEndTxnOnSubscriptionResponse());
					HandleEndTxnOnSubscriptionResponse(Cmd.EndTxnOnSubscriptionResponse);
					Cmd.EndTxnOnSubscriptionResponse.recycle();
					break;
				}
			}
			finally
			{
				if (CmdBuilder != null)
				{
					CmdBuilder.recycle();
				}

				if (Cmd != null)
				{
					Cmd.recycle();
				}

				Buffer.release();
			}
		}

		public abstract void MessageReceived();

		public virtual void HandlePartitionMetadataRequest(PulsarApi.CommandPartitionedTopicMetadata Response)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandlePartitionResponse(PulsarApi.CommandPartitionedTopicMetadataResponse Response)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleLookup(PulsarApi.CommandLookupTopic Lookup)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleLookupResponse(PulsarApi.CommandLookupTopicResponse Connection)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleConnect(PulsarApi.CommandConnect Connect)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleConnected(PulsarApi.CommandConnected Connected)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSubscribe(PulsarApi.CommandSubscribe Subscribe)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleProducer(PulsarApi.CommandProducer Producer)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSend(PulsarApi.CommandSend Send, ByteBuf HeadersAndPayload)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSendReceipt(PulsarApi.CommandSendReceipt SendReceipt)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSendError(PulsarApi.CommandSendError SendError)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleMessage(PulsarApi.CommandMessage CmdMessage, ByteBuf HeadersAndPayload)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAck(PulsarApi.CommandAck Ack)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleFlow(PulsarApi.CommandFlow Flow)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleRedeliverUnacknowledged(PulsarApi.CommandRedeliverUnacknowledgedMessages Redeliver)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleUnsubscribe(PulsarApi.CommandUnsubscribe Unsubscribe)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSeek(PulsarApi.CommandSeek Seek)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleActiveConsumerChange(PulsarApi.CommandActiveConsumerChange Change)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleSuccess(PulsarApi.CommandSuccess Success)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleProducerSuccess(PulsarApi.CommandProducerSuccess Success)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleError(PulsarApi.CommandError Error)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleCloseProducer(PulsarApi.CommandCloseProducer CloseProducer)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleCloseConsumer(PulsarApi.CommandCloseConsumer CloseConsumer)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandlePing(PulsarApi.CommandPing Ping)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandlePong(PulsarApi.CommandPong Pong)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleConsumerStats(PulsarApi.CommandConsumerStats CommandConsumerStats)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleConsumerStatsResponse(PulsarApi.CommandConsumerStatsResponse CommandConsumerStatsResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleReachedEndOfTopic(PulsarApi.CommandReachedEndOfTopic CommandReachedEndOfTopic)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetLastMessageId(PulsarApi.CommandGetLastMessageId GetLastMessageId)
		{
			throw new System.NotSupportedException();
		}
		public virtual void HandleGetLastMessageIdSuccess(PulsarApi.CommandGetLastMessageIdResponse Success)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespace(PulsarApi.CommandGetTopicsOfNamespace CommandGetTopicsOfNamespace)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespaceSuccess(PulsarApi.CommandGetTopicsOfNamespaceResponse Response)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetSchema(PulsarApi.CommandGetSchema CommandGetSchema)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetSchemaResponse(PulsarApi.CommandGetSchemaResponse CommandGetSchemaResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchema(PulsarApi.CommandGetOrCreateSchema CommandGetOrCreateSchema)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchemaResponse(PulsarApi.CommandGetOrCreateSchemaResponse CommandGetOrCreateSchemaResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAuthResponse(PulsarApi.CommandAuthResponse CommandAuthResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAuthChallenge(PulsarApi.CommandAuthChallenge CommandAuthChallenge)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleNewTxn(PulsarApi.CommandNewTxn CommandNewTxn)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleNewTxnResponse(PulsarApi.CommandNewTxnResponse CommandNewTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxn(PulsarApi.CommandAddPartitionToTxn CommandAddPartitionToTxn)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxn(PulsarApi.CommandAddSubscriptionToTxn CommandAddSubscriptionToTxn)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxnResponse(PulsarApi.CommandAddSubscriptionToTxnResponse CommandAddSubscriptionToTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxn(PulsarApi.CommandEndTxn CommandEndTxn)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxnResponse(PulsarApi.CommandEndTxnResponse CommandEndTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartition(PulsarApi.CommandEndTxnOnPartition CommandEndTxnOnPartition)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartitionResponse(PulsarApi.CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscription(PulsarApi.CommandEndTxnOnSubscription CommandEndTxnOnSubscription)
		{
			throw new System.NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscriptionResponse(PulsarApi.CommandEndTxnOnSubscriptionResponse CommandEndTxnOnSubscriptionResponse)
		{
			throw new System.NotSupportedException();
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarDecoder));
	}

}