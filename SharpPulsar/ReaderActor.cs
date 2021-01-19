using SharpPulsar.Interfaces;
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
	public class ReaderActor<T>
	{
		private static readonly BatchReceivePolicy _disabledBatchReceivePolicy = BatchReceivePolicy.Builder().Timeout(0, TimeUnit.MILLISECONDS).MaxNumMessages(1).Build();
		private readonly ConsumerImpl<T> _consumer;

		public ReaderActor(PulsarClientImpl client, ReaderConfigurationData<T> readerConfiguration, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> consumerFuture, Schema<T> schema)
		{

			string subscription = "reader-" + DigestUtils.sha1Hex(System.Guid.randomUUID().ToString()).substring(0, 10);
			if(StringUtils.isNotBlank(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
			consumerConfiguration.TopicNames.add(readerConfiguration.TopicName);
			consumerConfiguration.SubscriptionName = subscription;
			consumerConfiguration.SubscriptionType = SubscriptionType.Exclusive;
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
				ReaderListener<T> readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(this, readerListener);
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
			_consumer = new ConsumerImpl<T>(client, readerConfiguration.TopicName, consumerConfiguration, listenerExecutor, partitionIdx, false, consumerFuture, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, null, true);
		}

		private class MessageListenerAnonymousInnerClass : MessageListener<T>
		{
			private readonly ReaderActor<T> _outerInstance;

			private Org.Apache.Pulsar.Client.Api.IReaderListener<T> _readerListener;

			public MessageListenerAnonymousInnerClass(ReaderActor<T> outerInstance, Org.Apache.Pulsar.Client.Api.IReaderListener<T> readerListener)
			{
				this._outerInstance = outerInstance;
				this._readerListener = readerListener;
				_serialVersionUID = 1L;
			}

			private static readonly long _serialVersionUID;

			public void Received(ConsumerActor<T> consumer, Message<T> msg)
			{
				_readerListener.Received(_outerInstance, msg);
				consumer.AcknowledgeCumulativeAsync(msg);
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

		public virtual ConsumerImpl<T> Consumer
		{
			get
			{
				return _consumer;
			}
		}

		public virtual bool HasReachedEndOfTopic()
		{
			return _consumer.HasReachedEndOfTopic();
		}

		public virtual Message<T> ReadNext()
		{
			Message<T> msg = _consumer.Receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			_consumer.AcknowledgeCumulativeAsync(msg);
			return msg;
		}

		public virtual Message<T> ReadNext(int timeout, TimeUnit unit)
		{
			Message<T> msg = _consumer.Receive(timeout, unit);

			if(msg != null)
			{
				_consumer.AcknowledgeCumulativeAsync(msg);
			}
			return msg;
		}

		public virtual CompletableFuture<Message<T>> ReadNextAsync()
		{
			CompletableFuture<Message<T>> receiveFuture = _consumer.ReceiveAsync();
			receiveFuture.whenComplete((msg, t) =>
			{
			if(msg != null)
			{
				_consumer.acknowledgeCumulativeAsync(msg);
			}
			});
			return receiveFuture;
		}

		public override void close()
		{
			_consumer.close();
		}

		public virtual CompletableFuture<Void> CloseAsync()
		{
			return _consumer.CloseAsync();
		}

		public virtual bool HasMessageAvailable()
		{
			return _consumer.HasMessageAvailable();
		}

		public virtual CompletableFuture<bool> HasMessageAvailableAsync()
		{
			return _consumer.HasMessageAvailableAsync();
		}

		public virtual bool Connected
		{
			get
			{
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

		public virtual CompletableFuture<Void> SeekAsync(MessageId messageId)
		{
			return _consumer.SeekAsync(messageId);
		}

		public virtual CompletableFuture<Void> SeekAsync(long timestamp)
		{
			return _consumer.SeekAsync(timestamp);
		}
	}

}