/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Pulsar.Proto;
using SharpPulsar.Common.Precondition;
using SharpPulsar.Protocol.Schema;

namespace SharpPulsar.DotNetty
{   
    using static Akka.Actor.Status;

    /// <summary>
    /// Basic implementation of the channel handler to process inbound Pulsar data.
    /// <para>
    /// Please be aware that the decoded protocol command instance passed to a handle* method is cleared and reused for the
    /// next protocol command after the method completes. This is done in order to minimize object allocations for
    /// performance reasons. <b>It is not allowed to retain a reference to the handle* method parameter command instance
    /// after the method returns.</b> If you need to pass an instance of the command instance to another thread or retain a
    /// reference to it after the handle* method completes, you must make a deep copy of the command instance.
    /// </para>
    /// </summary>
    public abstract class PulsarDecoder : ChannelHandlerAdapter
    {

        // From the proxy protocol. If present, it means the client is connected via a reverse proxy.
        // The broker can get the real client address and proxy address from the proxy message.
        //protected internal HAProxyMessage proxyMessage;

        private readonly BaseCommand cmd = new BaseCommand();

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            /*if (msg is HAProxyMessage)
            {
                HAProxyMessage proxyMessage = (HAProxyMessage)msg;
                this.proxyMessage = proxyMessage;
                proxyMessage.release();
                return;
            }*/
            // Get a buffer that contains the full frame
            AbstractByteBuffer buffer = (AbstractByteBuffer)msg;
            try
            {
                // De-serialize the command
                int cmdSize = (int)buffer.ReadUnsignedInt();
                cmd.ParseFrom(buffer, cmdSize);

                if (log.isDebugEnabled())
                {
                    log.debug("[{}] Received cmd {}", ctx.Channel, cmd.GetType());
                }
                MessageReceived();

                switch (cmd.Type)
                {
                    case BaseCommand.Types.Type.PartitionedMetadata:
                        Condition.CheckArgument(cmd.PartitionMetadata != null);
                        try
                        {
                            InterceptCommand(cmd);
                            HandlePartitionMetadataRequest(cmd.PartitionMetadata);
                        }
                        catch (Exception e)
                        {
                            WriteAndFlush(ctx, Commands.NewPartitionMetadataResponse(GetServerError(e.InnerException), e.Message, cmd.PartitionMetadata.RequestId));
                        }
                        break;

                    case BaseCommand.Types.Type.PartitionedMetadataResponse:
                        Condition.CheckArgument(cmd.PartitionMetadataResponse.HasResponse);
                        HandlePartitionResponse(cmd.PartitionMetadataResponse);
                        break;

                    case BaseCommand.Types.Type.Lookup:
                        Condition.CheckArgument(cmd.LookupTopic.HasTopic);
                        HandleLookup(cmd.LookupTopic);
                        break;

                    case BaseCommand.Types.Type.LookupResponse:
                        Condition.CheckArgument(cmd.LookupTopicResponse.HasResponse);
                        HandleLookupResponse(cmd.LookupTopicResponse);
                        break;

                    case BaseCommand.Types.Type.Ack:
                        Condition.CheckArgument(cmd.Ack.HasAckType);
                        SafeInterceptCommand(cmd);
                        HandleAck(cmd.Ack);
                        break;

                    case BaseCommand.Types.Type.AckResponse:
                        Condition.CheckArgument(cmd.AckResponse.HasError);
                        HandleAckResponse(cmd.AckResponse);
                        break;

                    case BaseCommand.Types.Type.CloseConsumer:
                        Condition.CheckArgument(cmd.CloseConsumer.HasConsumerId);
                        SafeInterceptCommand(cmd);
                        HandleCloseConsumer(cmd.CloseConsumer);
                        break;

                    case BaseCommand.Types.Type.CloseProducer:
                        Condition.CheckArgument(cmd.CloseProducer.HasProducerId);
                        SafeInterceptCommand(cmd);
                        HandleCloseProducer(cmd.CloseProducer);
                        break;

                    case BaseCommand.Types.Type.Connect:
                        Condition.CheckArgument(cmd.Connect.HasAuthData);
                        HandleConnect(cmd.Connect);
                        break;

                    case BaseCommand.Types.Type.Connected:
                        Condition.CheckArgument(cmd.Connected.HasMaxMessageSize);
                        HandleConnected(cmd.Connected);
                        break;

                    case BaseCommand.Types.Type.Error:
                        Condition.CheckArgument(cmd.Error.HasError);
                        HandleError(cmd.Error);
                        break;

                    case BaseCommand.Types.Type.Flow:
                        Condition.CheckArgument(cmd.Flow.HasConsumerId);
                        HandleFlow(cmd.Flow);
                        break;

                    case BaseCommand.Types.Type.Message:
                        {
                            Condition.CheckArgument(cmd.Message != null);
                            HandleMessage(cmd.Message, buffer);
                            break;
                        }
                    case BaseCommand.Types.Type.Producer:
                        Condition.CheckArgument(cmd.Producer != null);
                        try
                        {
                            InterceptCommand(cmd);
                            HandleProducer(cmd.Producer);
                        }
                        catch (Exception e)
                        {
                            WriteAndFlush(ctx, Commands.NewError((long)cmd.Producer.RequestId, GetServerError(e.InnerException.HResult), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.Send:
                        {
                            Condition.CheckArgument(cmd.Send != null);
                            try
                            {
                                InterceptCommand(cmd);
                                // Store a buffer marking the content + headers
                                AbstractByteBuffer headersAndPayload = (AbstractByteBuffer)buffer.MarkReaderIndex();
                                HandleSend(cmd.Send, headersAndPayload);
                            }
                            catch (Exception e)
                            {
                                WriteAndFlush(ctx, Commands.NewSendError(cmd.Send.ProducerId, cmd.Send.SequenceId, GetServerError(e.getErrorCode()), e.Message));
                            }
                            break;
                        }
                    case BaseCommand.Types.Type.SendError:
                        Condition.CheckArgument(cmd.SendError.HasError);
                        HandleSendError(cmd.SendError);
                        break;

                    case BaseCommand.Types.Type.SendReceipt:
                        Condition.CheckArgument(cmd.SendReceipt != null);
                        HandleSendReceipt(cmd.SendReceipt);
                        break;

                    case BaseCommand.Types.Type.Subscribe:
                        Condition.CheckArgument(cmd.Subscribe != null);
                        try
                        {
                            InterceptCommand(cmd);
                            HandleSubscribe(cmd.Subscribe);
                        }
                        catch (Exception e)
                        {
                            WriteAndFlush(ctx, Commands.NewError((long)cmd.Subscribe.RequestId, GetServerError(e.HResult), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.Success:
                        checkArgument(cmd.hasSuccess());
                        handleSuccess(cmd.getSuccess());
                        break;

                    case BaseCommand.Types.Type.ProducerSuccess:
                        checkArgument(cmd.hasProducerSuccess());
                        handleProducerSuccess(cmd.getProducerSuccess());
                        break;

                    case BaseCommand.Types.Type.Unsubscribe:
                        checkArgument(cmd.hasUnsubscribe());
                        safeInterceptCommand(cmd);
                        handleUnsubscribe(cmd.getUnsubscribe());
                        break;

                    case BaseCommand.Types.Type.Seek:
                        checkArgument(cmd.hasSeek());
                        try
                        {
                            interceptCommand(cmd);
                            handleSeek(cmd.getSeek());
                        }
                        catch (InterceptException e)
                        {
                            writeAndFlush(ctx, Commands.newError(cmd.getSeek().getRequestId(), getServerError(e.getErrorCode()), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.Ping:
                        checkArgument(cmd.hasPing());
                        handlePing(cmd.getPing());
                        break;

                    case BaseCommand.Types.Type.Pong:
                        checkArgument(cmd.hasPong());
                        handlePong(cmd.getPong());
                        break;

                    case BaseCommand.Types.Type.RedeliverUnacknowledgedMessages:
                        checkArgument(cmd.hasRedeliverUnacknowledgedMessages());
                        safeInterceptCommand(cmd);
                        handleRedeliverUnacknowledged(cmd.getRedeliverUnacknowledgedMessages());
                        break;

                    case BaseCommand.Types.Type.ConsumerStats:
                        checkArgument(cmd.hasConsumerStats());
                        handleConsumerStats(cmd.getConsumerStats());
                        break;

                    case BaseCommand.Types.Type.ConsumerStatsResponse:
                        checkArgument(cmd.hasConsumerStatsResponse());
                        handleConsumerStatsResponse(cmd.getConsumerStatsResponse());
                        break;

                    case BaseCommand.Types.Type.ReachedEndOfTopic:
                        checkArgument(cmd.hasReachedEndOfTopic());
                        handleReachedEndOfTopic(cmd.getReachedEndOfTopic());
                        break;

                    case BaseCommand.Types.Type.TopicMigrated:
                        checkArgument(cmd.hasTopicMigrated());
                        handleTopicMigrated(cmd.getTopicMigrated());
                        break;

                    case BaseCommand.Types.Type.GetLastMessageId:
                        checkArgument(cmd.hasGetLastMessageId());
                        handleGetLastMessageId(cmd.getGetLastMessageId());
                        break;

                    case BaseCommand.Types.Type.GetLastMessageIdResponse:
                        checkArgument(cmd.hasGetLastMessageIdResponse());
                        handleGetLastMessageIdSuccess(cmd.getGetLastMessageIdResponse());
                        break;

                    case BaseCommand.Types.Type.ActiveConsumerChange:
                        handleActiveConsumerChange(cmd.getActiveConsumerChange());
                        break;

                    case BaseCommand.Types.Type.GetTopicsOfNamespace:
                        checkArgument(cmd.hasGetTopicsOfNamespace());
                        try
                        {
                            interceptCommand(cmd);
                            handleGetTopicsOfNamespace(cmd.getGetTopicsOfNamespace());
                        }
                        catch (InterceptException e)
                        {
                            writeAndFlush(ctx, Commands.newError(cmd.getGetTopicsOfNamespace().getRequestId(), getServerError(e.getErrorCode()), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.GetTopicsOfNamespaceResponse:
                        checkArgument(cmd.hasGetTopicsOfNamespaceResponse());
                        handleGetTopicsOfNamespaceSuccess(cmd.getGetTopicsOfNamespaceResponse());
                        break;

                    case BaseCommand.Types.Type.GetSchema:
                        checkArgument(cmd.hasGetSchema());
                        try
                        {
                            interceptCommand(cmd);
                            handleGetSchema(cmd.getGetSchema());
                        }
                        catch (InterceptException e)
                        {
                            writeAndFlush(ctx, Commands.newGetSchemaResponseError(cmd.getGetSchema().getRequestId(), getServerError(e.getErrorCode()), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.GetSchemaResponse:
                        checkArgument(cmd.hasGetSchemaResponse());
                        handleGetSchemaResponse(cmd.getGetSchemaResponse());
                        break;

                    case BaseCommand.Types.Type.GetOrCreateSchema:
                        checkArgument(cmd.hasGetOrCreateSchema());
                        try
                        {
                            interceptCommand(cmd);
                            handleGetOrCreateSchema(cmd.getGetOrCreateSchema());
                        }
                        catch (InterceptException e)
                        {
                            writeAndFlush(ctx, Commands.newGetOrCreateSchemaResponseError(cmd.getGetOrCreateSchema().getRequestId(), getServerError(e.getErrorCode()), e.Message));
                        }
                        break;

                    case BaseCommand.Types.Type.GetOrCreateSchemaResponse:
                        checkArgument(cmd.hasGetOrCreateSchemaResponse());
                        handleGetOrCreateSchemaResponse(cmd.getGetOrCreateSchemaResponse());
                        break;

                    case BaseCommand.Types.Type.AuthChallenge:
                        checkArgument(cmd.hasAuthChallenge());
                        handleAuthChallenge(cmd.getAuthChallenge());
                        break;

                    case BaseCommand.Types.Type.AuthResponse:
                        checkArgument(cmd.hasAuthResponse());
                        handleAuthResponse(cmd.getAuthResponse());
                        break;

                    case BaseCommand.Types.Type.TcClientConnectRequest:
                        checkArgument(cmd.hasTcClientConnectRequest());
                        handleTcClientConnectRequest(cmd.getTcClientConnectRequest());
                        break;

                    case BaseCommand.Types.Type.TcClientConnectResponse:
                        checkArgument(cmd.hasTcClientConnectResponse());
                        handleTcClientConnectResponse(cmd.getTcClientConnectResponse());
                        break;

                    case BaseCommand.Types.Type.NewTxn:
                        checkArgument(cmd.hasNewTxn());
                        handleNewTxn(cmd.getNewTxn());
                        break;

                    case BaseCommand.Types.Type.NewTxnResponse:
                        checkArgument(cmd.hasNewTxnResponse());
                        handleNewTxnResponse(cmd.getNewTxnResponse());
                        break;

                    case BaseCommand.Types.Type.AddPartitionToTxn:
                        checkArgument(cmd.hasAddPartitionToTxn());
                        handleAddPartitionToTxn(cmd.getAddPartitionToTxn());
                        break;

                    case BaseCommand.Types.Type.AddPartitionToTxnResponse:
                        checkArgument(cmd.hasAddPartitionToTxnResponse());
                        handleAddPartitionToTxnResponse(cmd.getAddPartitionToTxnResponse());
                        break;

                    case BaseCommand.Types.Type.AddSubscriptionToTxn:
                        checkArgument(cmd.hasAddSubscriptionToTxn());
                        handleAddSubscriptionToTxn(cmd.getAddSubscriptionToTxn());
                        break;

                    case BaseCommand.Types.Type.AddSubscriptionToTxnResponse:
                        checkArgument(cmd.hasAddSubscriptionToTxnResponse());
                        handleAddSubscriptionToTxnResponse(cmd.getAddSubscriptionToTxnResponse());
                        break;

                    case BaseCommand.Types.Type.EndTxn:
                        checkArgument(cmd.hasEndTxn());
                        handleEndTxn(cmd.getEndTxn());
                        break;

                    case BaseCommand.Types.Type.EndTxnResponse:
                        Condition.CheckArgument(cmd.EndTxnResponse != null);
                        HandleEndTxnResponse(cmd.EndTxnResponse);
                        break;

                    case BaseCommand.Types.Type.EndTxnOnPartition:
                        Condition.CheckArgument(cmd.EndTxnOnPartition != null);
                        HandleEndTxnOnPartition(cmd.EndTxnOnPartition);
                        break;

                    case BaseCommand.Types.Type.EndTxnOnPartitionResponse:
                        Condition.CheckArgument(cmd.EndTxnOnPartitionResponse != null);
                        HandleEndTxnOnPartitionResponse(cmd.EndTxnOnPartitionResponse);
                        break;

                    case BaseCommand.Types.Type.EndTxnOnSubscription:
                        Condition.CheckArgument(cmd.EndTxnOnSubscription != null);
                        HandleEndTxnOnSubscription(cmd.EndTxnOnSubscription);
                        break;

                    case BaseCommand.Types.Type.EndTxnOnSubscriptionResponse:
                        Condition.CheckArgument(cmd.EndTxnOnSubscriptionResponse != null);
                        HandleEndTxnOnSubscriptionResponse(cmd.EndTxnOnSubscriptionResponse);
                        break;

                    case BaseCommand.Types.Type.WatchTopicList:
                        Condition.CheckArgument(cmd.WatchTopicList != null);
                        HandleCommandWatchTopicList(cmd.WatchTopicList);
                        break;

                    case BaseCommand.Types.Type.WatchTopicListSuccess:
                        Condition.CheckArgument(cmd.WatchTopicListSuccess != null);
                        HandleCommandWatchTopicListSuccess(cmd.WatchTopicListSuccess);
                        break;

                    case BaseCommand.Types.Type.WatchTopicUpdate:
                        Condition.CheckArgument(cmd.WatchTopicUpdate != null);
                        HandleCommandWatchTopicUpdate(cmd.WatchTopicUpdate);
                        break;

                    case BaseCommand.Types.Type.WatchTopicListClose:
                        Condition.CheckArgument(cmd.WatchTopicListClose != null);
                        HandleCommandWatchTopicListClose(cmd.WatchTopicListClose);
                        break;

                    default:
                        break;
                }
            }
            finally
            {
                buffer.Release();
            }
        }

        protected internal abstract void MessageReceived();

        private ServerError GetServerError(int errorCode)
        {
            ServerError serverError = ServerError.ValueOf(errorCode);
            return serverError == null ? ServerError.UnknownError : serverError;
        }

        private void SafeInterceptCommand(BaseCommand command)
        {
            try
            {
                InterceptCommand(command);
            }
            catch (Exception)
            {
                // no-op
            }
        }

        protected internal virtual void InterceptCommand(BaseCommand command)
        {
            //No-op
        }

        protected internal virtual void HandlePartitionMetadataRequest(CommandPartitionedTopicMetadata response)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse response)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleLookup(CommandLookupTopic lookup)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleLookupResponse(CommandLookupTopicResponse connection)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleConnect(CommandConnect connect)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleConnected(CommandConnected connected)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSubscribe(CommandSubscribe subscribe)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleProducer(CommandProducer producer)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSend(CommandSend send, AbstractByteBuffer headersAndPayload)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSendReceipt(CommandSendReceipt sendReceipt)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSendError(CommandSendError sendError)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleMessage(CommandMessage cmdMessage, AbstractByteBuffer headersAndPayload)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAck(CommandAck ack)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAckResponse(CommandAckResponse ackResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleFlow(CommandFlow flow)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleRedeliverUnacknowledged(CommandRedeliverUnacknowledgedMessages redeliver)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleUnsubscribe(CommandUnsubscribe unsubscribe)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSeek(CommandSeek seek)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleActiveConsumerChange(CommandActiveConsumerChange change)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleSuccess(CommandSuccess success)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleProducerSuccess(CommandProducerSuccess success)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleError(CommandError error)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCloseProducer(CommandCloseProducer closeProducer)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandlePing(CommandPing ping)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandlePong(CommandPong pong)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleConsumerStats(CommandConsumerStats commandConsumerStats)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleConsumerStatsResponse(CommandConsumerStatsResponse commandConsumerStatsResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleTopicMigrated(CommandTopicMigrated commandMigratedTopic)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetLastMessageId(CommandGetLastMessageId getLastMessageId)
        {
            throw new System.NotSupportedException();
        }
        protected internal virtual void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetTopicsOfNamespace(CommandGetTopicsOfNamespace commandGetTopicsOfNamespace)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse response)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetSchema(CommandGetSchema commandGetSchema)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetOrCreateSchema(CommandGetOrCreateSchema commandGetOrCreateSchema)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAuthResponse(CommandAuthResponse commandAuthResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAuthChallenge(CommandAuthChallenge commandAuthChallenge)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleTcClientConnectRequest(CommandTcClientConnectRequest tcClientConnectRequest)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleTcClientConnectResponse(CommandTcClientConnectResponse tcClientConnectResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleNewTxn(CommandNewTxn commandNewTxn)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleNewTxnResponse(CommandNewTxnResponse commandNewTxnResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAddPartitionToTxn(CommandAddPartitionToTxn commandAddPartitionToTxn)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAddSubscriptionToTxn(CommandAddSubscriptionToTxn commandAddSubscriptionToTxn)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse commandAddSubscriptionToTxnResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxn(CommandEndTxn commandEndTxn)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxnResponse(CommandEndTxnResponse commandEndTxnResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxnOnPartition(CommandEndTxnOnPartition commandEndTxnOnPartition)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxnOnSubscription(CommandEndTxnOnSubscription commandEndTxnOnSubscription)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse commandEndTxnOnSubscriptionResponse)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCommandWatchTopicList(CommandWatchTopicList commandWatchTopicList)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCommandWatchTopicListSuccess(CommandWatchTopicListSuccess commandWatchTopicListSuccess)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCommandWatchTopicUpdate(CommandWatchTopicUpdate commandWatchTopicUpdate)
        {
            throw new System.NotSupportedException();
        }

        protected internal virtual void HandleCommandWatchTopicListClose(CommandWatchTopicListClose commandWatchTopicListClose)
        {
            throw new System.NotSupportedException();
        }

        private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarDecoder));

        private void WriteAndFlush(IChannelHandlerContext ctx, AbstractByteBuffer cmd)
        {
            NettyChannelUtil.writeAndFlushWithVoidPromise(ctx, cmd);
        }
    }
}
