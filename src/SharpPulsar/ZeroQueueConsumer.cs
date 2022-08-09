﻿using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Precondition;
using System;
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
    internal class ZeroQueueConsumer<T> : ConsumerActor<T>
	{
		private volatile bool _waitingOnReceiveForZeroQueueSize = false;
		private volatile bool _waitingOnListenerForZeroQueueSize = false;

		internal ZeroQueueConsumer(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : base(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, startMessageId, 0, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture)
		{
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

			Task.Run( () =>
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
					Listener.Received(_self, message);
				}
				catch (Exception t)
				{
					log.Error($"[{Topic}][{Subscription}] Message listener error in processing unqueued message: {message.MessageId} => {t}");
				}
				IncreaseAvailablePermits(_clientCnx);
				_waitingOnListenerForZeroQueueSize = false;
			});
		}

		protected internal override void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}
    }

}