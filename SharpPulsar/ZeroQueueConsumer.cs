using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Queues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		private volatile bool _waitingOnReceiveForZeroQueueSize = false;
		private volatile bool _waitingOnListenerForZeroQueueSize = false;

		public ZeroQueueConsumer(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> consumerQueue) : base(client, lookup, cnxPool, idGenerator, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration, consumerQueue)
		{
		}
		public static Props NewZeroQueueConsumer(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> consumerQueue)
        {
			return Props.Create(() => new ZeroQueueConsumer<T>(client, lookup, cnxPool, idGenerator, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration, consumerQueue));
        }
		protected internal override IMessage<T> InternalReceive()
		{
			var msg = FetchSingleMessageFromBroker();
			TrackMessage(msg);
			return BeforeConsume(msg);
		}

		private IMessage<T> FetchSingleMessageFromBroker()
		{
			// Just being cautious
			if(IncomingMessages.Count > 0)
			{
				_log.Error("The incoming message queue should never be greater than 0 when Queue size is 0");
				IncomingMessages = new System.Collections.Concurrent.BlockingCollection<IMessage<T>>();
			}

			IMessage<T> message;
			try
			{
				// if cnx is null or if the connection breaks the connectionOpened function will send the flow again
				_waitingOnReceiveForZeroQueueSize = true;
				if (Connected)
				{
					IncreaseAvailablePermits(Cnx());
				}
				do
				{
					message = IncomingMessages.Take();
					_lastDequeuedMessageId = message.MessageId;
					var msgCnx = ((Message<T>) message).Cnx();
					// synchronized need to prevent race between connectionOpened and the check "msgCnx == cnx()"
					// if message received due to an old flow - discard it and wait for the message from the
					// latest flow command
					if (msgCnx == Cnx())
					{
						_waitingOnReceiveForZeroQueueSize = false;
						break;
					}
				} while(true);

				Stats.UpdateNumMsgsReceived(message);
				return message;
			}
			catch(Exception e)
			{
				Stats.IncrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
			finally
			{
				// Finally blocked is invoked in case the block on incomingMessages is interrupted
				_waitingOnReceiveForZeroQueueSize = false;
				// Clearing the queue in case there was a race with messageReceived
				IncomingMessages = new System.Collections.Concurrent.BlockingCollection<IMessage<T>>();
			}
		}

		protected internal override void ConsumerIsReconnectedToBroker(IActorRef cnx, int currentQueueSize)
		{
			base.ConsumerIsReconnectedToBroker(cnx, currentQueueSize);

			// For zerosize queue : If the connection is reset and someone is waiting for the messages
			// or queue was not empty: send a flow command
			if(_waitingOnReceiveForZeroQueueSize || currentQueueSize > 0 || (Listener != null && !_waitingOnListenerForZeroQueueSize))
			{
				IncreaseAvailablePermits(cnx);
			}
		}

		protected internal override bool CanEnqueueMessage(IMessage<T> message)
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

		private void TriggerZeroQueueSizeListener(IMessage<T> message)
		{
			Condition.CheckNotNull(Listener, "listener can't be null");
			Condition.CheckNotNull(message, "unqueued message can't be null");

			Task.Run(() =>
			{
				var self = _self;
				var log = _log;
				Stats.UpdateNumMsgsReceived(message);
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug($"[{Topic}][{Subscription}] Calling message listener for unqueued message {message.MessageId}");
					}
					_waitingOnListenerForZeroQueueSize = true;
					TrackMessage(message);
					Listener.Received(self, BeforeConsume(message));
				}
				catch (Exception t)
				{
					log.Error($"[{Topic}][{Subscription}] Message listener error in processing unqueued message: {message.MessageId} => {t}");
				}
				IncreaseAvailablePermits(Cnx());
				_waitingOnListenerForZeroQueueSize = false;
			});
		}

		protected internal override void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}

		internal override void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, byte[] uncompressedPayload, MessageIdData messageId, IActorRef cnx)
		{
			_log.Warning($"Closing consumer [{Subscription}]-[{ConsumerName}] due to unsupported received batch-message with zero receiver queue size");
			// close connection
			cnx.GracefulStop(TimeSpan.FromSeconds(1));
		}
	}

}