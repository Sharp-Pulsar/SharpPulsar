using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Common;
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
namespace SharpPulsar
{
    internal class MultiTopicsReader<T> : ReceiveActor, IWithUnboundedStash
	{

		private readonly IActorRef _consumer;
		private readonly IActorRef _generator;

        public IStash Stash { get; set; }

        public MultiTopicsReader(IActorRef state, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ReaderConfigurationData<T> readerConfiguration, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ClientConfigurationData clientConfigurationData)
		{
			_generator = idGenerator;
			var subscription = "multiTopicsReader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
			if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}
			ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
			foreach(var topic in readerConfiguration.TopicNames)
				consumerConfiguration.TopicNames.Add(topic);

			consumerConfiguration.SubscriptionName = subscription;
			consumerConfiguration.SubscriptionType = SubType.Exclusive;
			consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
			consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
			consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

			if(readerConfiguration.ReaderListener != null)
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
			if(readerConfiguration.ResetIncludeHead)
			{
				consumerConfiguration.ResetIncludeHead = true;
			}
			consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
			if(readerConfiguration.CryptoKeyReader != null)
			{
				consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
			}
			if(readerConfiguration.KeyHashRanges != null)
			{
				consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
			}
			_consumer = Context.ActorOf(Props.Create(()=> new MultiTopicsConsumer<T>(state, client, lookup, cnxPool, _generator, consumerConfiguration, listenerExecutor, schema, true, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, clientConfigurationData)));

            ReceiveAsync<SubscribeAndCreateTopicsIfDoesNotExist>(async subs => 
            {
                await SubscribeAndCreateTopics(subs);
            });
            ReceiveAsync<Subscribe>(async sub => 
            {
                await SubscribeToTopic(sub);
            });
            ReceiveAny(_ => Stash.Stash());
		}
        private async ValueTask SubscribeAndCreateTopics(SubscribeAndCreateTopicsIfDoesNotExist subs)
        {
            var response = await _consumer.Ask<AskResponse>(subs).ConfigureAwait(false);
            Sender.Tell(response);
            if (!response.Failed)
            {
                Become(Ready);
            }
        }
        private async ValueTask SubscribeToTopic(Subscribe sub)
        {
            var response = await _consumer.Ask<AskResponse>(sub).ConfigureAwait(false);
            Sender.Tell(response);
            if (!response.Failed)
            {
                Become(Ready);
            }
        }
        private void Ready()
        {
            Receive<HasReachedEndOfTopic>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<AcknowledgeCumulativeMessage<T>>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<MessageProcessed<T>>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<HasMessageAvailable>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<GetTopic>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<IsConnected>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<SeekMessageId>(m => {
                _consumer.Tell(m, Sender);
            });
            Receive<SeekTimestamp>(m => {
                _consumer.Tell(m, Sender);
            });
            Stash?.UnstashAll();
        }
        private class MessageListenerAnonymousInnerClass : IMessageListener<T>
		{
			private readonly IActorRef _outerInstance;

			private IReaderListener<T> _readerListener;

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