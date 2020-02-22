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

using SharpPulsar.Utility.Protobuf;

namespace SharpPulsar.Protocol
{

    using DotNetty.Transport.Channels;
    using SharpPulsar.Protocol.Proto;
    using System;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Net;
    using DotNetty.Buffers;

    /// <summary>
    /// Basic implementation of the channel handler to process inbound Pulsar data.
    /// </summary>
    public abstract class PulsarDecoder : IChannelHandler
	{
		public void Read(IChannelHandlerContext ctx, object msg)
		{
			// Get a buffer that contains the full frame
			IByteBuffer buffer = (IByteBuffer) msg;
			BaseCommand cmd = null;
			BaseCommand.Builder cmdBuilder = null;

			try
			{
				// De-serialize the command
				int cmdSize = (int) buffer.ReadUnsignedInt();
				int writerIndex = buffer.WriterIndex;
				buffer.SetWriterIndex(buffer.ReaderIndex + cmdSize);
				ByteBufCodedInputStream cmdInputStream = ByteBufCodedInputStream.Get(buffer);
				cmdBuilder = BaseCommand.NewBuilder();
				cmd = ((BaseCommand.Builder)cmdBuilder.MergeFrom(cmdInputStream, null)).Build();
				buffer.SetWriterIndex(writerIndex);

				cmdInputStream.Recycle();

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Received cmd {}", ctx.Channel.RemoteAddress, cmd.Type);
				}

				MessageReceived();

				switch (cmd.Type)
				{
				case BaseCommand.Types.Type.PartitionedMetadata:
					if(cmd.HasPartitionMetadata)
						HandlePartitionMetadataRequest(cmd.PartitionMetadata);
					cmd.PartitionMetadata.Recycle();
					break;

				case BaseCommand.Types.Type.PartitionedMetadataResponse:
					if(cmd.HasPartitionMetadataResponse)
						HandlePartitionResponse(cmd.PartitionMetadataResponse);
					cmd.PartitionMetadataResponse.Recycle();
					break;

				case BaseCommand.Types.Type.Lookup:
					if(cmd.HasLookupTopic)
						HandleLookup(cmd.LookupTopic);
					cmd.LookupTopic.Recycle();
					break;

				case BaseCommand.Types.Type.LookupResponse:
					if(cmd.HasLookupTopicResponse)
						HandleLookupResponse(cmd.LookupTopicResponse);
					cmd.LookupTopicResponse.Recycle();
					break;

				case BaseCommand.Types.Type.Ack:
					if(cmd.HasAck)
					HandleAck(cmd.Ack);
					for (int i = 0; i < cmd.Ack.MessageId.Count; i++)
					{
						cmd.Ack.GetMessageId(i).Recycle();
					}
					cmd.Ack.Recycle();
					break;

				case BaseCommand.Types.Type.CloseConsumer:
					if(cmd.HasCloseConsumer)
						HandleCloseConsumer(cmd.CloseConsumer);
					cmd.CloseConsumer.Recycle();
					break;

				case BaseCommand.Types.Type.CloseProducer:
					if(cmd.HasCloseProducer)
						HandleCloseProducer(cmd.CloseProducer);
					cmd.CloseProducer.Recycle();
					break;

				case BaseCommand.Types.Type.Connect:
					if(cmd.HasConnect)
						HandleConnect(cmd.Connect);
					cmd.Connect.Recycle();
					break;
				case BaseCommand.Types.Type.Connected:
					if(cmd.HasConnected)
						HandleConnected(cmd.Connected);
					cmd.Connected.Recycle();
					break;

				case BaseCommand.Types.Type.Error:
					if(cmd.HasError)
						HandleError(cmd.Error);
					cmd.Error.Recycle();
					break;

				case BaseCommand.Types.Type.Flow:
					if(cmd.HasFlow)
						HandleFlow(cmd.Flow);
					cmd.Flow.Recycle();
					break;

				case BaseCommand.Types.Type.Message:
				{
					if(cmd.HasMessage)
						HandleMessage(cmd.Message, buffer);
					cmd.Message.Recycle();
					break;
				}
				case BaseCommand.Types.Type.Producer:
					if(cmd.HasProducer)
						HandleProducer(cmd.Producer);
					cmd.Producer.Recycle();
					break;

				case BaseCommand.Types.Type.Send:
				{
					if(cmd.HasSend)
							{
								// Store a buffer marking the content + headers
								IByteBuffer headersAndPayload = buffer.MarkReaderIndex();
								HandleSend(cmd.Send, headersAndPayload);
							}
					
					cmd.Send.Recycle();
					break;
				}
				case BaseCommand.Types.Type.SendError:
					if(cmd.HasSendError)
						HandleSendError(cmd.SendError);
					cmd.SendError.Recycle();
					break;

				case BaseCommand.Types.Type.SendReceipt:
					if(cmd.HasSendReceipt)
						HandleSendReceipt(cmd.SendReceipt);
					cmd.SendReceipt.Recycle();
					break;

				case BaseCommand.Types.Type.Subscribe:
					if(cmd.HasSubscribe)
						HandleSubscribe(cmd.Subscribe);
					cmd.Subscribe.Recycle();
					break;

				case BaseCommand.Types.Type.Success:
					if(cmd.HasSuccess)
						HandleSuccess(cmd.Success);
					cmd.Success.Recycle();
					break;

				case BaseCommand.Types.Type.ProducerSuccess:
					if(cmd.HasProducerSuccess)
						HandleProducerSuccess(cmd.ProducerSuccess);
					cmd.ProducerSuccess.Recycle();
					break;

				case BaseCommand.Types.Type.Unsubscribe:
					if(cmd.HasUnsubscribe)
						HandleUnsubscribe(cmd.Unsubscribe);
					cmd.Unsubscribe.Recycle();
					break;

				case BaseCommand.Types.Type.Seek:
					if(cmd.HasSeek)
						HandleSeek(cmd.Seek);
					cmd.Seek.Recycle();
					break;

				case BaseCommand.Types.Type.Ping:
					if(cmd.HasPing)
						HandlePing(cmd.Ping);
					cmd.Ping.Recycle();
					break;

				case BaseCommand.Types.Type.Pong:
					if(cmd.HasPong)
						HandlePong(cmd.Pong);
					cmd.Pong.Recycle();
					break;

				case BaseCommand.Types.Type.RedeliverUnacknowledgedMessages:
					if(cmd.HasRedeliverUnacknowledgedMessages)
						HandleRedeliverUnacknowledged(cmd.RedeliverUnacknowledgedMessages);
					cmd.RedeliverUnacknowledgedMessages.Recycle();
					break;

				case BaseCommand.Types.Type.ConsumerStats:
					if(cmd.HasConsumerStats)
						HandleConsumerStats(cmd.ConsumerStats);
					cmd.ConsumerStats.Recycle();
					break;

				case BaseCommand.Types.Type.ConsumerStatsResponse:
					if(cmd.HasConsumerStatsResponse)
						HandleConsumerStatsResponse(cmd.ConsumerStatsResponse);
					cmd.ConsumerStatsResponse.Recycle();
					break;

				case BaseCommand.Types.Type.ReachedEndOfTopic:
					if(cmd.HasReachedEndOfTopic)
						HandleReachedEndOfTopic(cmd.ReachedEndOfTopic);
					cmd.ReachedEndOfTopic.Recycle();
					break;

				case BaseCommand.Types.Type.GetLastMessageId:
					if(cmd.HasGetLastMessageId)
						HandleGetLastMessageId(cmd.GetLastMessageId);
					cmd.GetLastMessageId.Recycle();
					break;

				case BaseCommand.Types.Type.GetLastMessageIdResponse:
					if(cmd.HasGetLastMessageIdResponse)
						HandleGetLastMessageIdSuccess(cmd.GetLastMessageIdResponse);
					cmd.GetLastMessageIdResponse.Recycle();
					break;

				case BaseCommand.Types.Type.ActiveConsumerChange:
						HandleActiveConsumerChange(cmd.ActiveConsumerChange);
					cmd.ActiveConsumerChange.Recycle();
					break;

				case BaseCommand.Types.Type.GetTopicsOfNamespace:
					if(cmd.HasGetTopicsOfNamespace)
						HandleGetTopicsOfNamespace(cmd.GetTopicsOfNamespace);
					cmd.GetTopicsOfNamespace.Recycle();
					break;

				case BaseCommand.Types.Type.GetTopicsOfNamespaceResponse:
					if(cmd.HasGetTopicsOfNamespaceResponse)
						HandleGetTopicsOfNamespaceSuccess(cmd.GetTopicsOfNamespaceResponse);
					cmd.GetTopicsOfNamespaceResponse.Recycle();
					break;

				case BaseCommand.Types.Type.GetSchema:
					if(cmd.HasGetSchema)
						HandleGetSchema(cmd.GetSchema);
					cmd.GetSchema.Recycle();
					break;

				case BaseCommand.Types.Type.GetSchemaResponse:
					if(cmd.HasGetSchemaResponse)
					HandleGetSchemaResponse(cmd.GetSchemaResponse);
					cmd.GetSchemaResponse.Recycle();
					break;

				case BaseCommand.Types.Type.GetOrCreateSchema:
					if(cmd.HasGetOrCreateSchema)
						HandleGetOrCreateSchema(cmd.GetOrCreateSchema);
					cmd.GetOrCreateSchema.Recycle();
					break;

				case BaseCommand.Types.Type.GetOrCreateSchemaResponse:
					if(cmd.HasGetOrCreateSchemaResponse)
						HandleGetOrCreateSchemaResponse(cmd.GetOrCreateSchemaResponse);
					cmd.GetOrCreateSchemaResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AuthChallenge:
					if(cmd.HasAuthChallenge)
						HandleAuthChallenge(cmd.AuthChallenge);
					cmd.AuthChallenge.Recycle();
					break;

				case BaseCommand.Types.Type.AuthResponse:
					if(cmd.HasAuthResponse)
						HandleAuthResponse(cmd.AuthResponse);
					cmd.AuthResponse.Recycle();
					break;

				case BaseCommand.Types.Type.NewTxn:
					if(cmd.HasNewTxn)
						HandleNewTxn(cmd.NewTxn);
					cmd.NewTxn.Recycle();
					break;

				case BaseCommand.Types.Type.NewTxnResponse:
					if(cmd.HasNewTxnResponse)
						HandleNewTxnResponse(cmd.NewTxnResponse);
					cmd.NewTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AddPartitionToTxn:
					if(cmd.HasAddPartitionToTxn)
						HandleAddPartitionToTxn(cmd.AddPartitionToTxn);
					cmd.AddPartitionToTxn.Recycle();
					break;

				case BaseCommand.Types.Type.AddPartitionToTxnResponse:
					if(cmd.HasAddPartitionToTxnResponse)
						HandleAddPartitionToTxnResponse(cmd.AddPartitionToTxnResponse);
					cmd.AddPartitionToTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.AddSubscriptionToTxn:
					if(cmd.HasAddSubscriptionToTxn)
						HandleAddSubscriptionToTxn(cmd.AddSubscriptionToTxn);
					cmd.AddSubscriptionToTxn.Recycle();
					break;

				case BaseCommand.Types.Type.AddSubscriptionToTxnResponse:
					if(cmd.HasAddSubscriptionToTxnResponse)
						HandleAddSubscriptionToTxnResponse(cmd.AddSubscriptionToTxnResponse);
					cmd.AddSubscriptionToTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxn:
					if(cmd.HasEndTxn)
						HandleEndTxn(cmd.EndTxn);
					cmd.EndTxn.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnResponse:
					if(cmd.HasEndTxnResponse)
						HandleEndTxnResponse(cmd.EndTxnResponse);
					cmd.EndTxnResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnPartition:
					if(cmd.HasEndTxnOnPartition)
						HandleEndTxnOnPartition(cmd.EndTxnOnPartition);
					cmd.EndTxnOnPartition.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnPartitionResponse:
					if(cmd.HasEndTxnOnPartitionResponse)
						HandleEndTxnOnPartitionResponse(cmd.EndTxnOnPartitionResponse);
					cmd.EndTxnOnPartitionResponse.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnSubscription:
					if(cmd.HasEndTxnOnSubscription)
						HandleEndTxnOnSubscription(cmd.EndTxnOnSubscription);
					cmd.EndTxnOnSubscription.Recycle();
					break;

				case BaseCommand.Types.Type.EndTxnOnSubscriptionResponse:
					if(cmd.HasEndTxnOnSubscriptionResponse)
						HandleEndTxnOnSubscriptionResponse(cmd.EndTxnOnSubscriptionResponse);
					cmd.EndTxnOnSubscriptionResponse.Recycle();
					break;
				}
			}
			finally
			{
				if (cmdBuilder != null)
				{
					cmdBuilder.Recycle();
				}

				if (cmd != null)
				{
					cmd.Recycle();
				}

				buffer.Release();
			}
		}

		public abstract void MessageReceived();

		public virtual void HandlePartitionMetadataRequest(CommandPartitionedTopicMetadata response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleLookup(CommandLookupTopic lookup)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleLookupResponse(CommandLookupTopicResponse connection)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConnect(CommandConnect connect)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConnected(CommandConnected connected)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSubscribe(CommandSubscribe subscribe)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleProducer(CommandProducer producer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSend(CommandSend send, IByteBuffer headersAndPayload)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSendReceipt(CommandSendReceipt sendReceipt)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSendError(CommandSendError sendError)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleMessage(CommandMessage cmdMessage, IByteBuffer headersAndPayload)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAck(CommandAck ack)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleFlow(CommandFlow flow)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleRedeliverUnacknowledged(CommandRedeliverUnacknowledgedMessages redeliver)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleUnsubscribe(CommandUnsubscribe unsubscribe)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSeek(CommandSeek seek)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleActiveConsumerChange(CommandActiveConsumerChange change)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleSuccess(CommandSuccess success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleProducerSuccess(CommandProducerSuccess success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleError(CommandError error)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleCloseProducer(CommandCloseProducer closeProducer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePing(CommandPing ping)
		{
			throw new NotSupportedException();
		}

		public virtual void HandlePong(CommandPong pong)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConsumerStats(CommandConsumerStats commandConsumerStats)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleConsumerStatsResponse(CommandConsumerStatsResponse commandConsumerStatsResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetLastMessageId(CommandGetLastMessageId getLastMessageId)
		{
			throw new NotSupportedException();
		}
		public virtual void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespace(CommandGetTopicsOfNamespace commandGetTopicsOfNamespace)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse response)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetSchema(CommandGetSchema commandGetSchema)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchema(CommandGetOrCreateSchema commandGetOrCreateSchema)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAuthResponse(CommandAuthResponse commandAuthResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAuthChallenge(CommandAuthChallenge commandAuthChallenge)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleNewTxn(CommandNewTxn commandNewTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleNewTxnResponse(CommandNewTxnResponse commandNewTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxn(CommandAddPartitionToTxn commandAddPartitionToTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxn(CommandAddSubscriptionToTxn commandAddSubscriptionToTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse commandAddSubscriptionToTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxn(CommandEndTxn commandEndTxn)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnResponse(CommandEndTxnResponse commandEndTxnResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartition(CommandEndTxnOnPartition commandEndTxnOnPartition)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscription(CommandEndTxnOnSubscription commandEndTxnOnSubscription)
		{
			throw new NotSupportedException();
		}

		public virtual void HandleEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse commandEndTxnOnSubscriptionResponse)
		{
			throw new NotSupportedException();
		}

		public void ChannelRegistered(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelUnregistered(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelActive(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelInactive(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelRead(IChannelHandlerContext context, object message)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelReadComplete(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void ChannelWritabilityChanged(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void HandlerAdded(IChannelHandlerContext context)
		{
			Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void HandlerRemoved(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public Task WriteAsync(IChannelHandlerContext context, object message)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
		}

		public void Flush(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
		}

		public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
        }

		public Task DisconnectAsync(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
		}

		public Task CloseAsync(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
		}

		public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
            Console.WriteLine(exception.Message);
		}

		public Task DeregisterAsync(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
            return Task.CompletedTask;
		}

		public void Read(IChannelHandlerContext context)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		public void UserEventTriggered(IChannelHandlerContext context, object evt)
		{
            Console.WriteLine(context.Channel.RemoteAddress);
		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(PulsarDecoder));
	}

}