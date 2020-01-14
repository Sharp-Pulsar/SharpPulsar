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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using ByteBuf = io.netty.buffer.ByteBuf;


	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using Consumer = org.apache.pulsar.client.api.Consumer;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using org.apache.pulsar.client.impl.conf;
	using MessageIdData = org.apache.pulsar.common.api.proto.PulsarApi.MessageIdData;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class ZeroQueueConsumerImpl<T> extends ConsumerImpl<T>
	public class ZeroQueueConsumerImpl<T> : ConsumerImpl<T>
	{

		private readonly Lock zeroQueueLock = new ReentrantLock();

		private volatile bool waitingOnReceiveForZeroQueueSize = false;

		public ZeroQueueConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, int partitionIndex, bool hasParentConsumer, CompletableFuture<Consumer<T>> subscribeFuture, SubscriptionMode subscriptionMode, MessageId startMessageId, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeFuture, subscriptionMode, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Message<T> internalReceive() throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Message<T> internalReceive()
		{
			zeroQueueLock.@lock();
			try
			{
				return beforeConsume(fetchSingleMessageFromBroker());
			}
			finally
			{
				zeroQueueLock.unlock();
			}
		}

		protected internal override CompletableFuture<Message<T>> internalReceiveAsync()
		{
			CompletableFuture<Message<T>> future = base.internalReceiveAsync();
			if (!future.Done)
			{
				// We expect the message to be not in the queue yet
				sendFlowPermitsToBroker(cnx(), 1);
			}

			return future;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.pulsar.client.api.Message<T> fetchSingleMessageFromBroker() throws org.apache.pulsar.client.api.PulsarClientException
		private Message<T> fetchSingleMessageFromBroker()
		{
			// Just being cautious
			if (incomingMessages.size() > 0)
			{
				log.error("The incoming message queue should never be greater than 0 when Queue size is 0");
				incomingMessages.clear();
			}

			Message<T> message;
			try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				waitingOnReceiveForZeroQueueSize = true;
				lock (this)
				{
					if (Connected)
					{
						sendFlowPermitsToBroker(cnx(), 1);
					}
				}
				do
				{
					message = incomingMessages.take();
					lastDequeuedMessage = message.MessageId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ClientCnx msgCnx = ((MessageImpl<?>) message).getCnx();
					ClientCnx msgCnx = ((MessageImpl<object>) message).Cnx;
					// synchronized need to prevent race between connectionOpened and the check "msgCnx == cnx()"
					lock (this)
					{
						// if message received due to an old flow - discard it and wait for the message from the
						// latest flow command
						if (msgCnx == cnx())
						{
							waitingOnReceiveForZeroQueueSize = false;
							break;
						}
					}
				} while (true);

				stats.updateNumMsgsReceived(message);
				return message;
			}
			catch (InterruptedException e)
			{
				stats.incrementNumReceiveFailed();
				throw PulsarClientException.unwrap(e);
			}
			finally
			{
				// Finally blocked is invoked in case the block on incomingMessages is interrupted
				waitingOnReceiveForZeroQueueSize = false;
				// Clearing the queue in case there was a race with messageReceived
				incomingMessages.clear();
			}
		}

		protected internal override void consumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			base.consumerIsReconnectedToBroker(cnx, currentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if (waitingOnReceiveForZeroQueueSize || currentQueueSize > 0 || listener != null)
			{
				sendFlowPermitsToBroker(cnx, 1);
			}
		}

		protected internal override bool canEnqueueMessage(Message<T> message)
		{
			if (listener != null)
			{
				triggerZeroQueueSizeListener(message);
				return false;
			}
			else
			{
				return true;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void triggerZeroQueueSizeListener(final org.apache.pulsar.client.api.Message<T> message)
		private void triggerZeroQueueSizeListener(Message<T> message)
		{
			checkNotNull(listener, "listener can't be null");
			checkNotNull(message, "unqueued message can't be null");

			listenerExecutor.execute(() =>
			{
			stats.updateNumMsgsReceived(message);
			try
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}][{}] Calling message listener for unqueued message {}", topic, subscription, message.MessageId);
				}
				listener.received(ZeroQueueConsumerImpl.this, beforeConsume(message));
			}
			catch (Exception t)
			{
				log.error("[{}][{}] Message listener error in processing unqueued message: {}", topic, subscription, message.MessageId, t);
			}
			increaseAvailablePermits(cnx());
			});
		}

		protected internal override void triggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		internal override void receiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, ByteBuf uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			log.warn("Closing consumer [{}]-[{}] due to unsupported received batch-message with zero receiver queue size", subscription, consumerName);
			// close connection
			closeAsync().handle((ok, e) =>
			{
			notifyPendingReceivedCallback(null, new PulsarClientException.InvalidMessageException(format("Unsupported Batch message with 0 size receiver queue for [%s]-[%s] ", subscription, consumerName)));
			return null;
			});
		}
	}

}