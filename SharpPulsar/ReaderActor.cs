using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Queues;
using SharpPulsar.Utility;
using System;
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
	public class ReaderActor<T>: ReceiveActor
	{
		private static readonly BatchReceivePolicy _disabledBatchReceivePolicy = new BatchReceivePolicy.Builder().Timeout((int)TimeUnit.MILLISECONDS.ToMilliseconds(0)).MaxNumMessages(1).Build();
		private readonly IActorRef _consumer;

		public ReaderActor(IActorRef client, ReaderConfigurationData<T> readerConfiguration, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ClientConfigurationData clientConfigurationData, ConsumerQueueCollections<T> consumerQueue)
		{
			var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
			if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
			consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
			consumerConfiguration.SubscriptionName = subscription;
			consumerConfiguration.SubscriptionType = SubType.Exclusive;
			consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
			consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
			consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

			// Reader doesn't need any batch receiving behaviours
			// disable the batch receive timer for the ConsumerImpl instance wrapped by the ReaderImpl
			consumerConfiguration.BatchReceivePolicy = _disabledBatchReceivePolicy;

			if(readerConfiguration.ReaderName != null)
			{
				consumerConfiguration.ConsumerName = readerConfiguration.ReaderName;
			}

			if(readerConfiguration.ResetIncludeHead)
			{
				consumerConfiguration.ResetIncludeHead = true;
			}

			if(readerConfiguration.ReaderListener != null)
			{
				var readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(Self, readerListener);
			}

			consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
			if(readerConfiguration.CryptoKeyReader != null)
			{
				consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
			}

			if(readerConfiguration.KeyHashRanges != null)
			{
				consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().Ranges(readerConfiguration.KeyHashRanges);
			}

			int partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
			_consumer = Context.ActorOf(ConsumerActor<T>.NewConsumer(client, readerConfiguration.TopicName, consumerConfiguration, listenerExecutor, partitionIdx, false, readerConfiguration.StartMessageId, schema, null, true, readerConfiguration.StartMessageFromRollbackDurationInSec, clientConfigurationData, consumerQueue));
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

			private static readonly long _serialVersionUID;

			public void Received(IActorRef consumer, IMessage<T> msg)
			{
				_readerListener.Received(_outerInstance, msg);
				consumer.Tell(new AcknowledgeCumulativeMessage<T>(msg));
			}

			public void ReachedEndOfTopic(ConsumerActor<T> consumer)
			{
				_readerListener.ReachedEndOfTopic(_outerInstance);
			}
		}

		public virtual string Topic
		{
			get
			{
				return _consumer.Topic;
			}
		}

		public virtual IActorRef Consumer
		{
			get
			{
				return _consumer;
			}
		}

		public virtual bool HasReachedEndOfTopic()
		{
			//I dont need to do this cause the consumer will add the response to the consumer queue 
			//which will be read at the user side - use tell here and forget about it, the queue will have the response
			return _consumer.AskFor<bool>(Messages.Consumer.HasReachedEndOfTopic.Instance);
		}

		public virtual IMessage<T> ReadNext()
		{
			//I dont need to do this cause the consumer will add the response to the consumer queue 
			//which will be read at the user side - use tell here and forget about it, the queue will have the response
			Message<T> msg = _consumer.Receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			_consumer.AcknowledgeCumulativeAsync(msg);
			return msg;
		}

		public virtual IMessage<T> ReadNext(int timeout, TimeUnit unit)
		{
			//I dont need to do this cause the consumer will add the response to the consumer queue 
			//which will be read at the user side - use tell here and forget about it, the queue will have the response
			Message<T> msg = _consumer.Receive(timeout, unit);

			if(msg != null)
			{
				//user should acknowledge message to this actor who will further send it to the consumer
				_consumer.AcknowledgeCumulativeAsync(msg);
			}
			return msg;
		}
        protected override void PostStop()
        {
			_consumer.GracefulStop(TimeSpan.FromSeconds(1));
            base.PostStop();
        }

		public virtual bool HasMessageAvailable()
		{
			//I dont need to do this cause the consumer will add the response to the consumer queue 
			//which will be read at the user side - use tell here and forget about it, the queue will have the response
			return _consumer.HasMessageAvailable();
		}


		public virtual bool Connected
		{
			get
			{//I dont need to do this cause the consumer will add the response to the consumer queue 
			 //which will be read at the user side - use tell here and forget about it, the queue will have the response
				return _consumer.Connected;
			}
		}

		public virtual void Seek(MessageId messageId)
		{
			_consumer.Seek(messageId);
		}

		public virtual void Seek(long timestamp)
		{
			_consumer.Seek(timestamp);
		}

	}

}