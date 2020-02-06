using System;

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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using ByteBuf = io.netty.buffer.ByteBuf;


	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using Consumer = Api.IConsumer;
	using SharpPulsar.Api;
	using IMessageId = Api.IMessageId;
	using PulsarClientException = Api.PulsarClientException;
	using SharpPulsar.Api;
	using SharpPulsar.Impl.Conf;
	using MessageIdData = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageIdData;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class ZeroQueueConsumerImpl<T> extends ConsumerImpl<T>
	public class ZeroQueueConsumerImpl<T> : ConsumerImpl<T>
	{

		private readonly Lock zeroQueueLock = new ReentrantLock();

		private volatile bool waitingOnReceiveForZeroQueueSize = false;

		public ZeroQueueConsumerImpl(PulsarClientImpl Client, string Topic, ConsumerConfigurationData<T> Conf, ExecutorService ListenerExecutor, int PartitionIndex, bool HasParentConsumer, CompletableFuture<IConsumer<T>> SubscribeFuture, SubscriptionMode SubscriptionMode, IMessageId StartMessageId, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors, bool CreateTopicIfDoesNotExist) : base(Client, Topic, Conf, ListenerExecutor, PartitionIndex, HasParentConsumer, SubscribeFuture, SubscriptionMode, StartMessageId, 0, Schema, Interceptors, CreateTopicIfDoesNotExist)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Message<T> internalReceive() throws SharpPulsar.api.PulsarClientException
		public override Message<T> InternalReceive()
		{
			zeroQueueLock.@lock();
			try
			{
				return BeforeConsume(FetchSingleMessageFromBroker());
			}
			finally
			{
				zeroQueueLock.unlock();
			}
		}

		public override CompletableFuture<Message<T>> InternalReceiveAsync()
		{
			CompletableFuture<Message<T>> Future = base.InternalReceiveAsync();
			if (!Future.Done)
			{
				// We expect the message to be not in the queue yet
				SendFlowPermitsToBroker(Cnx(), 1);
			}

			return Future;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SharpPulsar.api.Message<T> fetchSingleMessageFromBroker() throws SharpPulsar.api.PulsarClientException
		private Message<T> FetchSingleMessageFromBroker()
		{
			// Just being cautious
			if (IncomingMessages.size() > 0)
			{
				log.error("The incoming message queue should never be greater than 0 when Queue size is 0");
				IncomingMessages.clear();
			}

			Message<T> Message;
			try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				waitingOnReceiveForZeroQueueSize = true;
				lock (this)
				{
					if (Connected)
					{
						SendFlowPermitsToBroker(Cnx(), 1);
					}
				}
				do
				{
					Message = IncomingMessages.take();
					LastDequeuedMessage = Message.MessageId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ClientCnx msgCnx = ((MessageImpl<?>) message).getCnx();
					ClientCnx MsgCnx = ((MessageImpl<object>) Message).Cnx;
					// synchronized need to prevent race between connectionOpened and the check "msgCnx == cnx()"
					lock (this)
					{
						// if message received due to an old flow - discard it and wait for the message from the
						// latest flow command
						if (MsgCnx == Cnx())
						{
							waitingOnReceiveForZeroQueueSize = false;
							break;
						}
					}
				} while (true);

				StatsConflict.updateNumMsgsReceived(Message);
				return Message;
			}
			catch (InterruptedException E)
			{
				StatsConflict.incrementNumReceiveFailed();
				throw PulsarClientException.unwrap(E);
			}
			finally
			{
				// Finally blocked is invoked in case the block on incomingMessages is interrupted
				waitingOnReceiveForZeroQueueSize = false;
				// Clearing the queue in case there was a race with messageReceived
				IncomingMessages.clear();
			}
		}

		public override void ConsumerIsReconnectedToBroker(ClientCnx Cnx, int CurrentQueueSize)
		{
			base.ConsumerIsReconnectedToBroker(Cnx, CurrentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if (waitingOnReceiveForZeroQueueSize || CurrentQueueSize > 0 || Listener != null)
			{
				SendFlowPermitsToBroker(Cnx, 1);
			}
		}

		public override bool CanEnqueueMessage(Message<T> Message)
		{
			if (Listener != null)
			{
				TriggerZeroQueueSizeListener(Message);
				return false;
			}
			else
			{
				return true;
			}
		}

		private void TriggerZeroQueueSizeListener(in Message<T> Message)
		{
			checkNotNull(Listener, "listener can't be null");
			checkNotNull(Message, "unqueued message can't be null");

			ListenerExecutor.execute(() =>
			{
			StatsConflict.updateNumMsgsReceived(Message);
			try
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}][{}] Calling message listener for unqueued message {}", Topic, SubscriptionConflict, Message.MessageId);
				}
				Listener.received(ZeroQueueConsumerImpl.this, BeforeConsume(Message));
			}
			catch (Exception T)
			{
				log.error("[{}][{}] Message listener error in processing unqueued message: {}", Topic, SubscriptionConflict, Message.MessageId, T);
			}
			IncreaseAvailablePermits(Cnx());
			});
		}

		public override void TriggerListener(int NumMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		public override void ReceiveIndividualMessagesFromBatch(MessageMetadata MsgMetadata, int RedeliveryCount, ByteBuf UncompressedPayload, MessageIdData MessageId, ClientCnx Cnx)
		{
			log.warn("Closing consumer [{}]-[{}] due to unsupported received batch-message with zero receiver queue size", SubscriptionConflict, ConsumerNameConflict);
			// close connection
			CloseAsync().handle((ok, e) =>
			{
			NotifyPendingReceivedCallback(null, new PulsarClientException.InvalidMessageException(format("Unsupported Batch message with 0 size receiver queue for [%s]-[%s] ", SubscriptionConflict, ConsumerNameConflict)));
			return null;
			});
		}
	}

}