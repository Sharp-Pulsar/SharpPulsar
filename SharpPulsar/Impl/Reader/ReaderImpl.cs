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
namespace org.apache.pulsar.client.impl
{
	using DigestUtils = org.apache.commons.codec.digest.DigestUtils;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using org.apache.pulsar.client.api;
	using SubscriptionMode = org.apache.pulsar.client.impl.ConsumerImpl.SubscriptionMode;
	using org.apache.pulsar.client.impl.conf;
	using org.apache.pulsar.client.impl.conf;
	using TopicName = org.apache.pulsar.common.naming.TopicName;

	public class ReaderImpl<T> : Reader<T>
	{

		private readonly ConsumerImpl<T> consumer;

		public ReaderImpl(PulsarClientImpl client, ReaderConfigurationData<T> readerConfiguration, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> consumerFuture, Schema<T> schema)
		{

			string subscription = "reader-" + DigestUtils.sha1Hex(System.Guid.randomUUID().ToString()).substring(0, 10);
			if (StringUtils.isNotBlank(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
			consumerConfiguration.TopicNames.add(readerConfiguration.TopicName);
			consumerConfiguration.SubscriptionName = subscription;
			consumerConfiguration.SubscriptionType = SubscriptionType.Exclusive;
			consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
			consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

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
				ReaderListener<T> readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(this, readerListener);
			}

			consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
			if (readerConfiguration.CryptoKeyReader != null)
			{
				consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
			}

			if (readerConfiguration.KeyHashRanges != null)
			{
				consumerConfiguration.KeySharedPolicy = KeySharedPolicy.stickyHashRange().ranges(readerConfiguration.KeyHashRanges);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int partitionIdx = org.apache.pulsar.common.naming.TopicName.getPartitionIndex(readerConfiguration.getTopicName());
			int partitionIdx = TopicName.getPartitionIndex(readerConfiguration.TopicName);
			consumer = new ConsumerImpl<T>(client, readerConfiguration.TopicName, consumerConfiguration, listenerExecutor, partitionIdx, false, consumerFuture, SubscriptionMode.NonDurable, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, null, true);
		}

		private class MessageListenerAnonymousInnerClass : MessageListener<T>
		{
			private readonly ReaderImpl<T> outerInstance;

			private ReaderListener<T> readerListener;

			public MessageListenerAnonymousInnerClass(ReaderImpl<T> outerInstance, ReaderListener<T> readerListener)
			{
				this.outerInstance = outerInstance;
				this.readerListener = readerListener;
				serialVersionUID = 1L;
			}

			private static readonly long serialVersionUID;

			public override void received(Consumer<T> consumer, Message<T> msg)
			{
				readerListener.received(outerInstance, msg);
				consumer.acknowledgeCumulativeAsync(msg);
			}

			public override void reachedEndOfTopic(Consumer<T> consumer)
			{
				readerListener.reachedEndOfTopic(outerInstance);
			}
		}

		public override string Topic
		{
			get
			{
				return consumer.Topic;
			}
		}

		public virtual ConsumerImpl<T> Consumer
		{
			get
			{
				return consumer;
			}
		}

		public override bool hasReachedEndOfTopic()
		{
			return consumer.hasReachedEndOfTopic();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public Message<T> readNext() throws PulsarClientException
		public override Message<T> readNext()
		{
			Message<T> msg = consumer.receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			consumer.acknowledgeCumulativeAsync(msg);
			return msg;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public Message<T> readNext(int timeout, java.util.concurrent.TimeUnit unit) throws PulsarClientException
		public override Message<T> readNext(int timeout, TimeUnit unit)
		{
			Message<T> msg = consumer.receive(timeout, unit);

			if (msg != null)
			{
				consumer.acknowledgeCumulativeAsync(msg);
			}
			return msg;
		}

		public override CompletableFuture<Message<T>> readNextAsync()
		{
			return consumer.receiveAsync().thenApply(msg =>
			{
			consumer.acknowledgeCumulativeAsync(msg);
			return msg;
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			consumer.close();
		}

		public override CompletableFuture<Void> closeAsync()
		{
			return consumer.closeAsync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean hasMessageAvailable() throws PulsarClientException
		public override bool hasMessageAvailable()
		{
			return consumer.hasMessageAvailable();
		}

		public override CompletableFuture<bool> hasMessageAvailableAsync()
		{
			return consumer.hasMessageAvailableAsync();
		}

		public override bool Connected
		{
			get
			{
				return consumer.Connected;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(MessageId messageId) throws PulsarClientException
		public override void seek(MessageId messageId)
		{
			consumer.seek(messageId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws PulsarClientException
		public override void seek(long timestamp)
		{
			consumer.seek(timestamp);
		}

		public override CompletableFuture<Void> seekAsync(MessageId messageId)
		{
			return consumer.seekAsync(messageId);
		}

		public override CompletableFuture<Void> seekAsync(long timestamp)
		{
			return consumer.seekAsync(timestamp);
		}
	}

}