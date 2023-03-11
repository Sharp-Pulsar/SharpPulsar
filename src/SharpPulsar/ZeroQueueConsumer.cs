using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
        
        public ZeroQueueConsumer(IActorRef client, long consumerId, IActorRef stateActor, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : 
            base(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, 0, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture)
		{
            InitReceiverQueueSize();
        }
        public ZeroQueueConsumer(IActorRef client, long consumerId, IActorRef stateActor, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
            :base(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, startMessageRollbackDurationInSec, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture)
        {
            InitReceiverQueueSize();    
        }
        public static Props Prop(IActorRef client, long consumerId, IActorRef stateActor,  IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ZeroQueueConsumer<T>(client, consumerId, stateActor, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
        }
        public static Props Prop(IActorRef client, long consumerId, IActorRef stateActor, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ZeroQueueConsumer<T>(client, consumerId, stateActor, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, startMessageRollbackDurationInSec, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
        }
        public override int MinReceiverQueueSize()
        {
            return 0;
        }

        protected override internal void InitReceiverQueueSize()
        {
            if (Conf.IsAutoScaledReceiverQueueSizeEnabled)
            {
                throw new NotImplementedException("AutoScaledReceiverQueueSize is not supported in ZeroQueueConsumerImpl");
            }
            else
            {
                CurrentReceiverQueueSize = 0;
            }
        }
        protected internal override IMessage<T> InternalReceive()
        {
            try
            {
                var msg = FetchSingleMessageFromBroker();
                TrackMessage(msg);
                return BeforeConsume(msg);
            }
            finally
            {
                
            }
        }
        protected internal override TaskCompletionSource<IMessage<T>> InternalReceiveAsync()
        {
            TaskCompletionSource<IMessage<T>> future = base.InternalReceiveAsync();
            if (!future.Task.IsCompleted)
            {
                // We expect the message to be not in the queue yet
                IncreaseAvailablePermits(Cnx());
            }

            return future;
        }
        private IMessage<T> FetchSingleMessageFromBroker()
        {
            // Just being cautious
            if (IncomingMessages.Count > 0)
            {
                _log.Error("The incoming message queue should never be greater than 0 when Queue size is 0");
                IncomingMessages.Empty();
            }

            IMessage<T> message;
            try
            {
                // if cnx is null or if the connection breaks the connectionOpened function will send the flow again
                _waitingOnReceiveForZeroQueueSize = true;
                if (Connected())
                {
                    IncreaseAvailablePermits(Cnx());
                }
                do
                {
                    if (IncomingMessages.TryReceive(out message))
                    {
                        _lastDequeuedMessageId = message.MessageId;
                        var msgCnx = ((Message<T>)message).Cnx();
                        // if message received due to an old flow - discard it and wait for the message from the
                        // latest flow command
                        if (msgCnx == Cnx())
                        {
                            _waitingOnReceiveForZeroQueueSize = false;
                            break;
                        }
                    }

                } while (true);

                Stats.UpdateNumMsgsReceived(message);
                return message;
            }
            catch (Exception e)
            {
                Stats.IncrementNumReceiveFailed();
                throw PulsarClientException.Unwrap(e);
            }
            finally
            {
                // Finally blocked is invoked in case the block on incomingMessages is interrupted
                _waitingOnReceiveForZeroQueueSize = false;
                // Clearing the queue in case there was a race with messageReceived
                IncomingMessages.Empty();
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
        }

		protected internal override void TriggerListener(int numMessages)
		{
			// Ignore since it was already triggered in the triggerZeroQueueSizeListener() call
		}
        internal override void ReceiveIndividualMessagesFromBatch(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, byte[] uncompressedPayload, MessageIdData messageId, IActorRef cnx, long consumerEpoch)
        {
            _log.Warning($"Closing consumer [{Subscription}]-[{ConsumerName}] due to unsupported received batch-message with zero receiver queue size");
            // close connection
            base.PostStop();
            NotifyPendingReceivedCallback(null, new PulsarClientException.InvalidMessageException($"Unsupported Batch message with 0 size receiver queue for [{Subscription}]-[{ConsumerName}] "));
        }

        private int CuentReceiverQueueSize
        {
            set
            {
                // receiver queue size is fixed as 0.

                throw new NotImplementedException("Receiver queue size can't be changed in ZeroQueueConsumerImpl");
            }
        }
    }

}