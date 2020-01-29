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

    using DotNetty.Transport.Channels;
    using SharpPulsar.Protocol.Proto;
    using System;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Net;
    using DotNetty.Buffers;
    using SharpPulsar.Util.Protobuf;

    /// <summary>
    /// Basic implementation of the channel handler to process inbound Pulsar data.
    /// </summary>
    public abstract class PulsarDecoder : IChannelHandler
	{
		public void Read(IChannelHandlerContext ctx, object Msg)
		{
			// Get a buffer that contains the full frame
			IByteBuffer buffer = (IByteBuffer) Msg;
			BaseCommand Cmd = null;
			BaseCommand.Builder CmdBuilder = null;

			try
			{
				// De-serialize the command
				int CmdSize = (int) buffer.ReadUnsignedInt();
				int WriterIndex = buffer.WriterIndex;
				buffer.WriterIndex(buffer.ReaderIndex + CmdSize);
				ByteBufCodedInputStream CmdInputStream = ByteBufCodedInputStream.Get(buffer);
				CmdBuilder = BaseCommand.newBuilder();
				Cmd = CmdBuilder.mergeFrom(CmdInputStream, null).build();
				buffer.writerIndex(WriterIndex);

				CmdInputStream.recycle();

				if (log.DebugEnabled)
				{
					log.debug("[{}] Received cmd {}", ctx.Channel.RemoteAddress, Cmd.getType());
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
					CommandAck Ack = Cmd.Ack;
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
					ByteBuf HeadersAndPayload = buffer.markReaderIndex();
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

				buffer.release();
			}
		}

		public abstract void MessageReceived();

		public virtual void HandlePartitionMetadataRequest(CommandPartitionedTopicMetadata Response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse Response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleLookup(CommandLookupTopic Lookup)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleLookupResponse(CommandLookupTopicResponse Connection)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConnect(CommandConnect Connect)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConnected(CommandConnected Connected)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSubscribe(CommandSubscribe Subscribe)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleProducer(CommandProducer Producer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSend(CommandSend Send, ByteBuf HeadersAndPayload)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSendReceipt(CommandSendReceipt SendReceipt)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSendError(CommandSendError SendError)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleMessage(CommandMessage CmdMessage, ByteBuf HeadersAndPayload)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAck(CommandAck Ack)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleFlow(CommandFlow Flow)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleRedeliverUnacknowledged(CommandRedeliverUnacknowledgedMessages Redeliver)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleUnsubscribe(CommandUnsubscribe Unsubscribe)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSeek(CommandSeek Seek)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleActiveConsumerChange(CommandActiveConsumerChange Change)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSuccess(CommandSuccess Success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleProducerSuccess(CommandProducerSuccess Success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleError(CommandError Error)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleCloseProducer(CommandCloseProducer CloseProducer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleCloseConsumer(CommandCloseConsumer CloseConsumer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePing(CommandPing Ping)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePong(CommandPong Pong)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConsumerStats(CommandConsumerStats CommandConsumerStats)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConsumerStatsResponse(CommandConsumerStatsResponse CommandConsumerStatsResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleReachedEndOfTopic(CommandReachedEndOfTopic CommandReachedEndOfTopic)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetLastMessageId(CommandGetLastMessageId GetLastMessageId)
		{
			throw new NotSupportedException();
		}
		public virtual void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse Success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespace(CommandGetTopicsOfNamespace CommandGetTopicsOfNamespace)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse Response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetSchema(CommandGetSchema CommandGetSchema)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetSchemaResponse(CommandGetSchemaResponse CommandGetSchemaResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchema(CommandGetOrCreateSchema CommandGetOrCreateSchema)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse CommandGetOrCreateSchemaResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAuthResponse(CommandAuthResponse CommandAuthResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAuthChallenge(CommandAuthChallenge CommandAuthChallenge)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleNewTxn(CommandNewTxn CommandNewTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleNewTxnResponse(CommandNewTxnResponse CommandNewTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxn(CommandAddPartitionToTxn CommandAddPartitionToTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxn(CommandAddSubscriptionToTxn CommandAddSubscriptionToTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse CommandAddSubscriptionToTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxn(CommandEndTxn CommandEndTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnResponse(CommandEndTxnResponse CommandEndTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartition(CommandEndTxnOnPartition CommandEndTxnOnPartition)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscription(CommandEndTxnOnSubscription CommandEndTxnOnSubscription)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse CommandEndTxnOnSubscriptionResponse)
		{
			throw new NotSupportedException();
		}

		public void ChannelRegistered(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ChannelUnregistered(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ChannelActive(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ChannelInactive(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ChannelRead(IChannelHandlerContext context, object message)
		{
			throw new NotImplementedException();
		}

		public void ChannelReadComplete(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ChannelWritabilityChanged(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void HandlerAdded(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void HandlerRemoved(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(IChannelHandlerContext context, object message)
		{
			throw new NotImplementedException();
		}

		public void Flush(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
		{
			throw new NotImplementedException();
		}

		public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
		{
			throw new NotImplementedException();
		}

		public Task DisconnectAsync(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public Task CloseAsync(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
			throw new NotImplementedException();
		}

		public Task DeregisterAsync(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void Read(IChannelHandlerContext context)
		{
			throw new NotImplementedException();
		}

		public void UserEventTriggered(IChannelHandlerContext context, object evt)
		{
			throw new NotImplementedException();
		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(PulsarDecoder));
	}

}