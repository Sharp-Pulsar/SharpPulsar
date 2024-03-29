﻿using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
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
namespace SharpPulsar.Consumer
{
    internal class ReaderActor<T> : ReceiveActor
    {
        private static readonly BatchReceivePolicy _disabledBatchReceivePolicy = new BatchReceivePolicy.Builder().Timeout(0, TimeUnit.TimeUnit.MILLISECONDS).MaxNumMessages(1).Build();
        private readonly IActorRef _consumer;
        private readonly IActorRef _generator;

        public ReaderActor(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ReaderConfigurationData<T> readerConfiguration, ISchema<T> schema, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            _generator = idGenerator;
            var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }

            var consumerConfiguration = new ConsumerConfigurationData<T>();
            consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = SubType.Exclusive;
            consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

            // chunking configuration
            consumerConfiguration.MaxPendingChuckedMessage = readerConfiguration.MaxPendingChunkedMessage;
            consumerConfiguration.AutoAckOldestChunkedMessageOnQueueFull = readerConfiguration.AutoAckOldestChunkedMessageOnQueueFull;
            consumerConfiguration.ExpireTimeOfIncompleteChunkedMessage = TimeSpan.FromMilliseconds(readerConfiguration.ExpireTimeOfIncompleteChunkedMessage);


            // Reader doesn't need any batch receiving behaviours
            // disable the batch receive timer for the ConsumerImpl instance wrapped by the ReaderImpl
            consumerConfiguration.BatchReceivePolicy = _disabledBatchReceivePolicy;

            if (readerConfiguration.StartMessageId != null)
                consumerConfiguration.StartMessageId = (BatchMessageId)readerConfiguration.StartMessageId;


            if (readerConfiguration.ReaderName != null)
            {
                consumerConfiguration.ConsumerName = readerConfiguration.ReaderName;
            }

            if (readerConfiguration.ResetIncludeHead)
            {
                consumerConfiguration.ResetIncludeHead = true;
            }

            if (readerConfiguration.ReaderListener != null)
            {
                var readerListener = readerConfiguration.ReaderListener;
                consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(Self, readerListener);
            }

            consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
            if (readerConfiguration.CryptoKeyReader != null)
            {
                consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
            }

            if (readerConfiguration.KeyHashRanges != null)
            {
                consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
            }
            var consumerInterceptors = ReaderInterceptorUtil.ConvertToConsumerInterceptors(Self, readerConfiguration.ReaderInterceptorList);
            var partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
            consumerConfiguration.Interceptors = consumerInterceptors;
            _consumer = Context.ActorOf(ConsumerActor<T>.Prop(consumerId, stateActor, client, lookup, cnxPool, _generator, readerConfiguration.TopicName, consumerConfiguration, partitionIdx, false, false, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, true, clientConfigurationData, subscribeFuture));
            Receive<HasReachedEndOfTopic>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<AcknowledgeCumulativeMessage<T>>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<Messages.Consumer.Receive>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<Connect>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<MessageProcessed<T>>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<HasMessageAvailable>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<GetTopic>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<IsConnected>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<SeekMessageId>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<SeekTimestamp>(m =>
            {
                _consumer.Forward(m);
            });
        }

        public static Props Prop(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ReaderConfigurationData<T> readerConfiguration, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ReaderActor<T>(consumerId, stateActor, client, lookup, cnxPool, idGenerator, readerConfiguration, schema, clientConfigurationData, subscribeFuture));
        }
        private class MessageListenerAnonymousInnerClass : IMessageListener<T>
        {
            private readonly IActorRef _outerInstance;

            private readonly IReaderListener<T> _readerListener;

            public MessageListenerAnonymousInnerClass(IActorRef outerInstance, IReaderListener<T> readerListener)
            {
                _outerInstance = outerInstance;
                _readerListener = readerListener;
            }


            public void Received(IActorRef consumer, IMessage<T> msg)
            {
                _readerListener.Received(_outerInstance, msg);
                consumer.Tell(new AcknowledgeCumulativeMessage<T>(msg));
            }

            public void ReachedEndOfTopic(IActorRef consumer)
            {
                _readerListener.ReachedEndOfTopic(_outerInstance);
            }
        }

        protected override void PostStop()
        {
            _consumer.GracefulStop(TimeSpan.FromSeconds(1));
            base.PostStop();
        }

    }

}