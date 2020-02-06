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
				buffer.SetWriterIndex(buffer.ReaderIndex + CmdSize);
				ByteBufCodedInputStream CmdInputStream = ByteBufCodedInputStream.Get(buffer);
				CmdBuilder = BaseCommand.NewBuilder();
				Cmd = CmdBuilder.MergeFrom(CmdInputStream, null).Build();
				buffer.SetWriterIndex(WriterIndex);

				CmdInputStream.Recycle();

				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Received cmd {}", ctx.Channel.RemoteAddress, Cmd.Type);
				}

				MessageReceived();

				switch (Cmd.Type)
				{
				case BaseCommand.Types.Type.PartitionedMetadata:
					if(Cmd.HasPartitionMetadata)
						HandlePartitionMetadataRequest(Cmd.PartitionMetadata);
					Cmd.PartitionMetadata.Recycle();
					break;

				case BaseCommand.Types.Type.PartitionedMetadataResponse:
					if(Cmd.HasPartitionMetadataResponse)
						HandlePartitionResponse(Cmd.PartitionMetadataResponse);
					Cmd.PartitionMetadataResponse.Recycle();
					break;

				case BaseCommand.Types.Type.Lookup:
					if(Cmd.HasLookupTopic)
						HandleLookup(Cmd.LookupTopic);
					Cmd.LookupTopic.Recycle();
					break;

				case BaseCommand.Types.Type.LookupResponse:
					if(Cmd.HasLookupTopicResponse)
						HandleLookupResponse(Cmd.LookupTopicResponse);
					Cmd.LookupTopicResponse.Recycle();
					break;

				case BaseCommand.Types.Type.Ack:
					if(Cmd.HasAck)
					HandleAck(Cmd.Ack);
					for (int I = 0; I < Cmd.Ack.MessageId.Count; I++)
					{
						Cmd.Ack.GetMessageId(I).Recycle();
					}
					Cmd.Ack.Recycle();
					break;

				case BaseCommand.Types.Type.CloseConsumer:
					if(Cmd.HasCloseConsumer)
						HandleCloseConsumer(Cmd.CloseConsumer);
					Cmd.CloseConsumer.Recycle();
					break;

				case BaseCommand.Types.Type.CloseProducer:
					if(Cmd.HasCloseProducer)
						HandleCloseProducer(Cmd.CloseProducer);
					Cmd.CloseProducer.Recycle();
					break;

				case BaseCommand.Types.Type.Connect:
					if(Cmd.HasConnect)
						HandleConnect(Cmd.Connect);
					Cmd.Connect.Recycle();
					break;
				case BaseCommand.Types.Type.Connected:
					if(Cmd.HasConnected)
						HandleConnected(Cmd.Connected);
					Cmd.Connected.Recycle();
					break;

				case BaseCommand.Types.Type.Error:
					if(Cmd.HasError)
						HandleError(Cmd.Error);
					Cmd.Error.Recycle();
					break;

				case BaseCommand.Types.Type.Flow:
					if(Cmd.HasFlow)
						HandleFlow(Cmd.Flow);
					Cmd.Flow.Recycle();
					break;

				case BaseCommand.Types.Type.Message:
				{
					if(Cmd.HasMessage)
						HandleMessage(Cmd.Message, buffer);
					Cmd.Message.Recycle();
					break;
				}
				case BaseCommand.Types.Type.Producer:
					if(Cmd.HasProducer)
						HandleProducer(Cmd.Producer);
					Cmd.Producer.Recycle();
					break;

				case BaseCommand.Types.Type.Send:
				{
					if(Cmd.HasSend)
							{
								// Store a buffer marking the content + headers
								IByteBuffer headersAndPayload = buffer.MarkReaderIndex();
								HandleSend(Cmd.Send, headersAndPayload);
							}
					
					Cmd.Send.Recycle();
					break;
				}
				case BaseCommand.Types.Type.SendError:
					if(Cmd.HasSendError)
						HandleSendError(Cmd.SendError);
					Cmd.SendError.Recycle();
					break;

				case BaseCommand.Types.Type.SendReceipt:
					if(Cmd.HasSendReceipt)
						HandleSendReceipt(Cmd.SendReceipt);
					Cmd.SendReceipt.Recycle();
					break;

				case BaseCommand.Types.Type.Subscribe:
					if(Cmd.HasSubscribe)
						HandleSubscribe(Cmd.Subscribe);
					Cmd.Subscribe.Recycle();
					break;

				case BaseCommand.Types.Type.Success:
					if(Cmd.HasSuccess)
						HandleSuccess(Cmd.Success);
					Cmd.Success.Recycle();
					break;

				case BaseCommand.Types.Type.ProducerSuccess:
					if(Cmd.HasProducerSuccess)
						HandleProducerSuccess(Cmd.ProducerSuccess);
					Cmd.ProducerSuccess.Recycle();
					break;

				case BaseCommand.Types.Type.Unsubscribe:
					if(Cmd.HasUnsubscribe)
						HandleUnsubscribe(Cmd.Unsubscribe);
					Cmd.Unsubscribe.Recycle();
					break;

				case BaseCommand.Types.Type.Seek:
					if(Cmd.HasSeek)
						HandleSeek(Cmd.Seek);
					Cmd.Seek.Recycle();
					break;

				case BaseCommand.Types.Type.Ping:
					if(Cmd.HasPing)
						HandlePing(Cmd.Ping);
					Cmd.Ping.Recycle();
					break;

				case BaseCommand.Types.Type.Pong:
					if(Cmd.HasPong)
						HandlePong(Cmd.Pong);
					Cmd.Pong.Recycle();
					break;

				case BaseCommand.Types.Type.RedeliverUnacknowledgedMessages:
					if(Cmd.HasRedeliverUnacknowledgedMessages)
						HandleRedeliverUnacknowledged(Cmd.RedeliverUnacknowledgedMessages);
					Cmd.RedeliverUnacknowledgedMessages.Recycle();
					break;

				case BaseCommand.Types.Type.ConsumerStats:
					if(Cmd.HasConsumerStats)
						HandleConsumerStats(Cmd.ConsumerStats);
					Cmd.ConsumerStats.Recycle();
					break;

				case BaseCommand.Types.Type.ConsumerStatsResponse:
					if(Cmd.HasConsumerStatsResponse)
						HandleConsumerStatsResponse(Cmd.ConsumerStatsResponse);
					Cmd.ConsumerStatsResponse.Recycle();
					break;

				case BaseCommand.Types.Type.ReachedEndOfTopic:
					if(Cmd.HasReachedEndOfTopic)
						HandleReachedEndOfTopic(Cmd.ReachedEndOfTopic);
					Cmd.ReachedEndOfTopic.Recycle();
					break;

				case BaseCommand.Types.Type.GetLastMessageId:
					if(Cmd.HasGetLastMessageId)
						HandleGetLastMessageId(Cmd.GetLastMessageId);
					Cmd.GetLastMessageId.Recycle();
					break;

				case BaseCommand.Types.Type.GetLastMessageIdResponse:
					if(Cmd.HasGetLastMessageIdResponse)
						HandleGetLastMessageIdSuccess(Cmd.GetLastMessageIdResponse);
					Cmd.GetLastMessageIdResponse.Recycle();
					break;

				case BaseCommand.Types.Type.ActiveConsumerChange:
						HandleActiveConsumerChange(Cmd.ActiveConsumerChange);
					Cmd.ActiveConsumerChange.Recycle();
					break;

				case BaseCommand.Types.Type.GetTopicsOfNamespace:
					if(Cmd.HasGetTopicsOfNamespace)
						HandleGetTopicsOfNamespace(Cmd.GetTopicsOfNamespace);
					Cmd.GetTopicsOfNamespace.Recycle();
					break;

				case BaseCommand.Types.Type.GetTopicsOfNamespaceResponse:
					if(Cmd.HasGetTopicsOfNamespaceResponse)
						HandleGetTopicsOfNamespaceSuccess(Cmd.GetTopicsOfNamespaceResponse);
					Cmd.GetTopicsOfNamespaceResponse.Recycle();
					break;

				case BaseCommand.Types.Type.GetSchema:
					if(Cmd.HasGetSchema)
						HandleGetSchema(Cmd.GetSchema);
					Cmd.GetSchema.Recycle();
					break;

				case BaseCommand.Types.Type.GetSchemaResponse:
					if(Cmd.HasGetSchemaResponse)
					HandleGetSchemaResponse(Cmd.GetSchemaResponse);
					Cmd.GetSchemaResponse.Recycle();
					break;

				case BaseCommand.Types.Type.GetOrCreateSchema:
					if(Cmd.HasGetOrCreateSchema)
						HandleGetOrCreateSchema(Cmd.GetOrCreateSchema);
					Cmd.GetOrCreateSchema.Recycle();
					break;

				case BaseCommand.Types.Type.GetOrCreateSchemaResponse:
					if(Cmd.HasGetOrCreateSchemaResponse)
						HandleGetOrCreateSchemaResponse(Cmd.GetOrCreateSchemaResponse);
					Cmd.GetOrCreateSchemaResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AuthChallenge:
					if(Cmd.HasAuthChallenge)
						HandleAuthChallenge(Cmd.AuthChallenge);
					Cmd.AuthChallenge.Recycle();
					break;

				case BaseCommand.Types.Type.AuthResponse:
					if(Cmd.HasAuthResponse)
						HandleAuthResponse(Cmd.AuthResponse);
					Cmd.AuthResponse.Recycle();
					break;

				case BaseCommand.Types.Type.NewTxn:
					if(Cmd.HasNewTxn)
						HandleNewTxn(Cmd.NewTxn);
					Cmd.NewTxn.Recycle();
					break;

				case BaseCommand.Types.Type.NewTxnResponse:
					if(Cmd.HasNewTxnResponse)
						HandleNewTxnResponse(Cmd.NewTxnResponse);
					Cmd.NewTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AddPartitionToTxn:
					if(Cmd.HasAddPartitionToTxn)
						HandleAddPartitionToTxn(Cmd.AddPartitionToTxn);
					Cmd.AddPartitionToTxn.Recycle();
					break;

				case BaseCommand.Types.Type.AddPartitionToTxnResponse:
					if(Cmd.HasAddPartitionToTxnResponse)
						HandleAddPartitionToTxnResponse(Cmd.AddPartitionToTxnResponse);
					Cmd.AddPartitionToTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AddSubscriptionToTxn:
					if(Cmd.HasAddSubscriptionToTxn)
						HandleAddSubscriptionToTxn(Cmd.AddSubscriptionToTxn);
					Cmd.AddSubscriptionToTxn.Recycle();
					break;

				case BaseCommand.Types.Type.AddSubscriptionToTxnResponse:
					if(Cmd.HasAddSubscriptionToTxnResponse)
						HandleAddSubscriptionToTxnResponse(Cmd.AddSubscriptionToTxnResponse);
					Cmd.AddSubscriptionToTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxn:
					if(Cmd.HasEndTxn)
						HandleEndTxn(Cmd.EndTxn);
					Cmd.EndTxn.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnResponse:
					if(Cmd.HasEndTxnResponse)
						HandleEndTxnResponse(Cmd.EndTxnResponse);
					Cmd.EndTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnPartition:
					if(Cmd.HasEndTxnOnPartition)
						HandleEndTxnOnPartition(Cmd.EndTxnOnPartition);
					Cmd.EndTxnOnPartition.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnPartitionResponse:
					if(Cmd.HasEndTxnOnPartitionResponse)
						HandleEndTxnOnPartitionResponse(Cmd.EndTxnOnPartitionResponse);
					Cmd.EndTxnOnPartitionResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnSubscription:
					if(Cmd.HasEndTxnOnSubscription)
						HandleEndTxnOnSubscription(Cmd.EndTxnOnSubscription);
					Cmd.EndTxnOnSubscription.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnSubscriptionResponse:
					if(Cmd.HasEndTxnOnSubscriptionResponse)
						HandleEndTxnOnSubscriptionResponse(Cmd.EndTxnOnSubscriptionResponse);
					Cmd.EndTxnOnSubscriptionResponse.Recycle();
					break;
				}
			}
			finally
			{
				if (CmdBuilder != null)
				{
					CmdBuilder.Recycle();
				}

				if (Cmd != null)
				{
					Cmd.Recycle();
				}

				buffer.Release();
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

		public virtual void HandleSend(CommandSend Send, IByteBuffer HeadersAndPayload)
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

		public virtual void HandleMessage(CommandMessage CmdMessage, IByteBuffer HeadersAndPayload)
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