using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Util;
using SharpPulsar.Util.Atomic.Locking;

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
	public class ZeroQueueConsumerImpl<T> : ConsumerImpl<T>
	{

		private readonly ILock _zeroQueueLock = new ReentrantLock();

		private volatile bool _waitingOnReceiveForZeroQueueSize = false;

		public ZeroQueueConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ScheduledThreadPoolExecutor listenerExecutor, int partitionIndex, bool hasParentConsumer, TaskCompletionSource<IConsumer<T>> subscribeTask, SubscriptionMode subscriptionMode, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeTask, subscriptionMode, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist)
		{
		}

        public override IMessage<T> InternalReceive()
		{
			_zeroQueueLock.Lock();
			try
			{
				return BeforeConsume(FetchSingleMessageFromBroker());
			}
			finally
			{
				_zeroQueueLock.Unlock();
			}
		}

		public override ValueTask<IMessage<T>> InternalReceiveAsync()
		{
			var task = base.InternalReceiveAsync();
			if (!task.IsCompleted)
			{
				// We expect the message to be not in the queue yet
				SendFlowPermitsToBroker(Cnx(), 1);
			}

			return task;
		}

		private IMessage<T> FetchSingleMessageFromBroker()
		{
			// Just being cautious
			if (IncomingMessages.size() > 0)
			{
				Log.LogError("The incoming message queue should never be greater than 0 when Queue size is 0");
				IncomingMessages.clear();
			}

            try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				_waitingOnReceiveForZeroQueueSize = true;
				lock (this)
				{
					if (Connected)
					{
						SendFlowPermitsToBroker(Cnx(), 1);
					}
				}

                IMessage<T> message;
                do
				{
					message = IncomingMessages.take();
					LastDequeuedMessage = message.MessageId;
					var msgCnx = ((MessageImpl<object>) message).Cnx;
					// synchronized need to prevent race between connectionOpened and the check "msgCnx == cnx()"
					lock (this)
					{
						// if message received due to an old flow - discard it and wait for the message from the
						// latest flow command
						if (msgCnx == Cnx())
						{
							_waitingOnReceiveForZeroQueueSize = false;
							break;
						}
					}
				} while (true);

				ConsumerStats.UpdateNumMsgsReceived(message);
				return message;
			}
			catch (ThreadInterruptedException e)
			{
				ConsumerStats.IncrementNumReceiveFailed();
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

		public new void ConsumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			base.ConsumerIsReconnectedToBroker(cnx, currentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if (_waitingOnReceiveForZeroQueueSize || currentQueueSize > 0 || Listener != null)
			{
				SendFlowPermitsToBroker(cnx, 1);
			}
		}

		public override bool CanEnqueueMessage(IMessage<T> message)
		{
			if (Listener != null)
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
			if(Listener == null)
                throw new NullReferenceException("listener can't be null");
			if(message == null)
                throw new NullReferenceException("unqueued message can't be null");

			Task.Run(() =>
			{
			    ConsumerStats.UpdateNumMsgsReceived(message);
			    try
			    {
				    if (Log.IsEnabled(LogLevel.Debug))
				    {
					    Log.LogDebug("[{}][{}] Calling message listener for unqueued message {}", Topic, Subscription, message.MessageId);
				    }
				    Listener.Received(this, BeforeConsume(message));
			    }
			    catch (System.Exception T)
			    {
				    Log.LogError("[{}][{}] Message listener error in processing unqueued message: {}", Topic, Subscription, message.MessageId, T);
			    }
			    IncreaseAvailablePermits(Cnx());
			});
		}

		public new void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		public new void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IByteBuffer uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			Log.LogWarning("Closing consumer [{}]-[{}] due to unsupported received batch-message with zero receiver queue size", Subscription, ConsumerName);
			// close connection
			CloseAsync().AsTask().ContinueWith( t=>
			{
			    NotifyPendingReceivedCallback(null, new PulsarClientException.InvalidMessageException(string.Format("Unsupported Batch message with 0 size receiver queue for [%s]-[%s] ", Subscription, ConsumerName)));
			    //return null;
			});
		}
	}

}