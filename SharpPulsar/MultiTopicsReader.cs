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
	public class MultiTopicsReader<T> : ReaderActor<T>
	{

		private readonly MultiTopicsConsumer<T> _multiTopicsConsumer;

		public MultiTopicsReader(PulsarClientImpl client, ReaderConfigurationData<T> readerConfiguration, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> consumerFuture, Schema<T> schema)
		{
			string subscription = "multiTopicsReader-" + DigestUtils.sha1Hex(System.Guid.randomUUID().ToString()).substring(0, 10);
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
			consumerConfiguration.TopicNames.add(readerConfiguration.TopicName);

			if(readerConfiguration.ReaderListener != null)
			{
				ReaderListener<T> readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(this, readerListener);
			}

			if(readerConfiguration.ReaderName != null)
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
				consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().Ranges(readerConfiguration.KeyHashRanges);
			}
			_multiTopicsConsumer = new MultiTopicsConsumerImpl<T>(client, consumerConfiguration, listenerExecutor, consumerFuture, schema, null, true, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec);
		}

		private class MessageListenerAnonymousInnerClass : MessageListener<T>
		{
			private readonly MultiTopicsReader<T> _outerInstance;

			private ReaderListener<T> _readerListener;

			public MessageListenerAnonymousInnerClass(MultiTopicsReader<T> outerInstance, ReaderListener<T> readerListener)
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
				return _multiTopicsConsumer.Topic;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> readNext() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual Message<T> ReadNext()
		{
			Message<T> msg = _multiTopicsConsumer.Receive();
			_multiTopicsConsumer.TryAcknowledgeMessage(msg);
			return msg;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> readNext(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual Message<T> ReadNext(int timeout, TimeUnit unit)
		{
			Message<T> msg = _multiTopicsConsumer.Receive(timeout, unit);
			_multiTopicsConsumer.TryAcknowledgeMessage(msg);
			return msg;
		}

		public virtual CompletableFuture<Message<T>> ReadNextAsync()
		{
			return _multiTopicsConsumer.ReceiveAsync().thenApply(msg =>
			{
			_multiTopicsConsumer.acknowledgeCumulativeAsync(msg);
			return msg;
			});
		}

		public virtual CompletableFuture<Void> CloseAsync()
		{
			return _multiTopicsConsumer.CloseAsync();
		}

		public virtual bool HasReachedEndOfTopic()
		{
			return _multiTopicsConsumer.HasReachedEndOfTopic();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean hasMessageAvailable() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual bool HasMessageAvailable()
		{
			return _multiTopicsConsumer.HasMessageAvailable() || _multiTopicsConsumer.NumMessagesInQueue() > 0;
		}

		public virtual CompletableFuture<bool> HasMessageAvailableAsync()
		{
			return _multiTopicsConsumer.HasMessageAvailableAsync();
		}

		public virtual bool Connected
		{
			get
			{
				return _multiTopicsConsumer.Connected;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Seek(MessageId messageId)
		{
			_multiTopicsConsumer.Seek(messageId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Seek(long timestamp)
		{
			_multiTopicsConsumer.Seek(timestamp);
		}

		public virtual CompletableFuture<Void> SeekAsync(MessageId messageId)
		{
			return _multiTopicsConsumer.SeekAsync(messageId);
		}

		public virtual CompletableFuture<Void> SeekAsync(long timestamp)
		{
			return _multiTopicsConsumer.SeekAsync(timestamp);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			_multiTopicsConsumer.close();
		}

		public virtual MultiTopicsConsumer<T> MultiTopicsConsumer
		{
			get
			{
				return _multiTopicsConsumer;
			}
		}
	}

}