using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;

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
namespace SharpPulsar
{
	public class ZeroQueueConsumer<T> : ConsumerActor<T>
	{

		private readonly Lock _zeroQueueLock = new ReentrantLock();

		private volatile bool _waitingOnReceiveForZeroQueueSize = false;
		private volatile bool _waitingOnListenerForZeroQueueSize = false;

		public ZeroQueueConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration) : base(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration)
		{
		}
		public static Props NewZeroQueueConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration)
        {
			return Props.Create(() => new ZeroQueueConsumer<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration));
        }
		protected internal override Message<T> InternalReceive()
		{
			_zeroQueueLock.@lock();
			try
			{
				Message<T> msg = FetchSingleMessageFromBroker();
				TrackMessage(msg);
				return BeforeConsume(msg);
			}
			finally
			{
				_zeroQueueLock.unlock();
			}
		}

		protected internal override CompletableFuture<Message<T>> InternalReceiveAsync()
		{
			CompletableFuture<Message<T>> future = base.InternalReceiveAsync();
			if(!future.Done)
			{
				// We expect the message to be not in the queue yet
				IncreaseAvailablePermits(Cnx());
			}

			return future;
		}

		private Message<T> FetchSingleMessageFromBroker()
		{
			// Just being cautious
			if(IncomingMessages.size() > 0)
			{
				log.error("The incoming message queue should never be greater than 0 when Queue size is 0");
				IncomingMessages.clear();
			}

			Message<T> message;
			try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				_waitingOnReceiveForZeroQueueSize = true;
				lock(this)
				{
					if(Connected)
					{
						IncreaseAvailablePermits(Cnx());
					}
				}
				do
				{
					message = IncomingMessages.take();
					_lastDequeuedMessageId = message.MessageId;
					ClientCnx msgCnx = ((MessageImpl<object>) message).Cnx;
					// synchronized need to prevent race between connectionOpened and the check "msgCnx == cnx()"
					lock(this)
					{
						// if message received due to an old flow - discard it and wait for the message from the
						// latest flow command
						if(msgCnx == Cnx())
						{
							_waitingOnReceiveForZeroQueueSize = false;
							break;
						}
					}
				} while(true);

				StatsConflict.UpdateNumMsgsReceived(message);
				return message;
			}
			catch(InterruptedException e)
			{
				StatsConflict.IncrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
			finally
			{
				// Finally blocked is invoked in case the block on incomingMessages is interrupted
				_waitingOnReceiveForZeroQueueSize = false;
				// Clearing the queue in case there was a race with messageReceived
				IncomingMessages.clear();
			}
		}

		protected internal override void ConsumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			base.ConsumerIsReconnectedToBroker(cnx, currentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if(_waitingOnReceiveForZeroQueueSize || currentQueueSize > 0 || (Listener != null && !_waitingOnListenerForZeroQueueSize))
			{
				IncreaseAvailablePermits(cnx);
			}
		}

		protected internal override bool CanEnqueueMessage(Message<T> message)
		{
			if(Listener != null)
			{
				TriggerZeroQueueSizeListener(message);
				return false;
			}
			else
			{
				return true;
			}
		}

		private void TriggerZeroQueueSizeListener(in Message<T> message)
		{
			checkNotNull(Listener, "listener can't be null");
			checkNotNull(message, "unqueued message can't be null");

			ListenerExecutor.execute(() =>
			{
			StatsConflict.UpdateNumMsgsReceived(message);
			try
			{
				if(log.DebugEnabled)
				{
					log.debug("[{}][{}] Calling message listener for unqueued message {}", Topic, SubscriptionConflict, message.MessageId);
				}
				_waitingOnListenerForZeroQueueSize = true;
				TrackMessage(message);
				Listener.Received(ZeroQueueConsumer.this, BeforeConsume(message));
			}
			catch(Exception t)
			{
				log.error("[{}][{}] Message listener error in processing unqueued message: {}", Topic, SubscriptionConflict, message.MessageId, t);
			}
			IncreaseAvailablePermits(Cnx());
			_waitingOnListenerForZeroQueueSize = false;
			});
		}

		protected internal override void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		internal override void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, ByteBuf uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
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