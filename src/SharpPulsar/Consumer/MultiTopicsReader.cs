﻿using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
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
    internal class MultiTopicsReader<T> : ReceiveActor, IWithUnboundedStash
    {

        private readonly IActorRef _consumer;
        private IActorRef _sender;
        private readonly IActorRef _generator;

        public IStash Stash { get; set; }

        public MultiTopicsReader(IActorRef state, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ReaderConfigurationData<T> readerConfiguration, ISchema<T> schema, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            _generator = idGenerator;
            string subscription;
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionName))
            {
                subscription = readerConfiguration.SubscriptionName;
            }
            else
            {
                subscription = "multiTopicsReader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
                if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
                {
                    subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
                }
            }

            var consumerConfiguration = new ConsumerConfigurationData<T>();
            foreach (var topic in readerConfiguration.TopicNames)
                consumerConfiguration.TopicNames.Add(topic);

            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = SubType.Exclusive;
            consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

            // chunking configuration
            consumerConfiguration.MaxPendingChuckedMessage = readerConfiguration.MaxPendingChunkedMessage;
            consumerConfiguration.AutoAckOldestChunkedMessageOnQueueFull = readerConfiguration.AutoAckOldestChunkedMessageOnQueueFull;
            consumerConfiguration.ExpireTimeOfIncompleteChunkedMessage = TimeSpan.FromMilliseconds(readerConfiguration.ExpireTimeOfIncompleteChunkedMessage);


            if (readerConfiguration.ReaderListener != null)
            {
                var readerListener = readerConfiguration.ReaderListener;
                consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(Self, readerListener);
            }
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
            consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
            if (readerConfiguration.CryptoKeyReader != null)
            {
                consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
            }
            if (readerConfiguration.KeyHashRanges != null)
            {
                consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
            }
            if (readerConfiguration.AutoUpdatePartitions)
            {
                consumerConfiguration.AutoUpdatePartitionsInterval = readerConfiguration.AutoUpdatePartitionsInterval;
            }
            _consumer = Context.ActorOf(Props.Create(() => new MultiTopicsConsumer<T>(state, client, lookup, cnxPool, _generator, consumerConfiguration, schema, true, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, clientConfigurationData, subscribeFuture)));

            ReceiveAsync<Subscribe>(async sub =>
            {
                _sender = Sender;
                await SubscribeToTopic(sub);
            });
            Receive<HasReachedEndOfTopic>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<AcknowledgeCumulativeMessage<T>>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<MessageProcessed<T>>(m =>
            {
                _consumer.Forward(m);
            });
            Receive<Messages.Consumer.Receive>(m =>
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
            ReceiveAny(m =>
            {
                _consumer.Forward(m);
            });
        }
        public static Props Prop(IActorRef state, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ReaderConfigurationData<T> readerConfiguration, ISchema<T> schema, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new MultiTopicsReader<T>(state, client, lookup, cnxPool, idGenerator, readerConfiguration, schema, clientConfigurationData, subscribeFuture));
        }
        private async ValueTask SubscribeToTopic(Subscribe sub)
        {
            try
            {
                var response = await _consumer.Ask<AskResponse>(sub).ConfigureAwait(false);
                _sender.Tell(response);
            }
            catch (Exception ex)
            {
                _sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
            }
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
            _consumer.GracefulStop(TimeSpan.FromSeconds(5));

            base.PostStop();
        }
    }

}