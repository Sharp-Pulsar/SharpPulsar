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
    using SharpPulsar.Util.Atomic.Locking;
    using SharpPulsar.Configuration;
    using System.Threading.Tasks;
    using SharpPulsar.Interface.Consumer;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Interface.Message;
    using SharpPulsar.Impl.Message;
    using SharpPulsar.Common.PulsarApi;
    using SharpPulsar.Exception;

    public class ZeroQueueConsumerImpl<T> : ConsumerImpl<T>
	{

		private readonly ReentrantLock zeroQueueLock = new ReentrantLock();

		private volatile bool waitingOnReceiveForZeroQueueSize = false;

		public ZeroQueueConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, int partitionIndex, bool hasParentConsumer, ValueTask<IConsumer<T>> subscribeFuture, SubscriptionMode subscriptionMode, MessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeFuture, subscriptionMode, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist)
		{
		}

		protected internal override IMessage<T> InternalReceive()
		{
			zeroQueueLock.Lock();
			try
			{
				return BeforeConsume(FetchSingleMessageFromBroker());
			}
			finally
			{
				zeroQueueLock.Unlock();
			}
		}

		protected internal override ValueTask<IMessage<T>> InternalReceiveAsync()
		{
			ValueTask<IMessage<T>> future = base.internalReceiveAsync();
			if (!future.IsCompleted)
			{
				// We expect the message to be not in the queue yet
				SendFlowPermitsToBroker(cnx(), 1);
			}

			return future;
		}

		private IMessage<T> FetchSingleMessageFromBroker()
		{
			// Just being cautious
			if (incomingMessages.size() > 0)
			{
				log.error("The incoming message queue should never be greater than 0 when Queue size is 0");
				incomingMessages.clear();
			}

			IMessage<T> message;
			try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				waitingOnReceiveForZeroQueueSize = true;
				lock (this)
				{
					if (Connected)
					{
						SendFlowPermitsToBroker(cnx(), 1);
					}
				}
				do
				{
					message = incomingMessages.take();
					lastDequeuedMessage = message.MessageId;
					ClientConnection msgCnx = ((MessageImpl<T>) message).Cnx;
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

				stats.updateNumMsgsReceived<T>(message);
				return message;
			}
			catch (InterruptedException e)
			{
				stats.incrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
			finally
			{
				// Finally blocked is invoked in case the block on incomingMessages is interrupted
				waitingOnReceiveForZeroQueueSize = false;
				// Clearing the queue in case there was a race with messageReceived
				incomingMessages.clear();
			}
		}

		protected internal void ConsumerIsReconnectedToBroker(ClientConnection cnx, int currentQueueSize)
		{
			base.ConsumerIsReconnectedToBroker(cnx, currentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if (waitingOnReceiveForZeroQueueSize || currentQueueSize > 0 || listener != null)
			{
				SendFlowPermitsToBroker(cnx, 1);
			}
		}

		protected internal override bool CanEnqueueMessage(IMessage<T> message)
		{
			if (listener != null)
			{
				TriggerZeroQueueSizeListener(message);
				return false;
			}
			else
			{
				return true;
			}
		}

		private void TriggerZeroQueueSizeListener(IMessage<T> message)
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

		protected internal void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		internal override void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, ByteBuf uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			log.warn("Closing consumer [{}]-[{}] due to unsupported received batch-message with zero receiver queue size", subscription, consumerName);
			// close connection
			CloseAsync().handle((ok, e) =>
			{
				notifyPendingReceivedCallback(null, new PulsarClientException.InvalidMessageException(format("Unsupported Batch message with 0 size receiver queue for [%s]-[%s] ", subscription, consumerName)));
				return null;
			});
		}
	}

}