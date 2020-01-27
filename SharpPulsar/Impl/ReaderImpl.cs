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
namespace SharpPulsar.Impl
{
	using DigestUtils = org.apache.commons.codec.digest.DigestUtils;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using SharpPulsar.Api;
	using SubscriptionMode = SharpPulsar.Impl.ConsumerImpl.SubscriptionMode;
	using SharpPulsar.Impl.Conf;
	using SharpPulsar.Impl.Conf;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
    using System.Threading.Tasks;

    public class ReaderImpl<T> : IReader<T>
	{

		private readonly ConsumerImpl<T> consumer;

		public ReaderImpl(PulsarClientImpl Client, ReaderConfigurationData<T> ReaderConfiguration, TaskCompletionSource<IConsumer<T>> ConsumerFuture, ISchema<T> Schema)
		{

			string Subscription = "reader-" + DigestUtils.sha1Hex(System.Guid.randomUUID().ToString()).substring(0, 10);
			if (StringUtils.isNotBlank(ReaderConfiguration.SubscriptionRolePrefix))
			{
				Subscription = ReaderConfiguration.SubscriptionRolePrefix + "-" + Subscription;
			}

			ConsumerConfigurationData<T> ConsumerConfiguration = new ConsumerConfigurationData<T>();
			ConsumerConfiguration.TopicNames.add(ReaderConfiguration.TopicName);
			ConsumerConfiguration.SubscriptionName = Subscription;
			ConsumerConfiguration.SubscriptionType = SubscriptionType.Exclusive;
			ConsumerConfiguration.ReceiverQueueSize = ReaderConfiguration.ReceiverQueueSize;
			ConsumerConfiguration.ReadCompacted = ReaderConfiguration.ReadCompacted;

			if (ReaderConfiguration.ReaderName != null)
			{
				ConsumerConfiguration.ConsumerName = ReaderConfiguration.ReaderName;
			}

			if (ReaderConfiguration.ResetIncludeHead)
			{
				ConsumerConfiguration.ResetIncludeHead = true;
			}

			if (ReaderConfiguration.ReaderListener != null)
			{
				ReaderListener<T> ReaderListener = ReaderConfiguration.ReaderListener;
				ConsumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(this, ReaderListener);
			}

			ConsumerConfiguration.CryptoFailureAction = ReaderConfiguration.CryptoFailureAction;
			if (ReaderConfiguration.CryptoKeyReader != null)
			{
				ConsumerConfiguration.CryptoKeyReader = ReaderConfiguration.CryptoKeyReader;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int partitionIdx = org.apache.pulsar.common.naming.TopicName.getPartitionIndex(readerConfiguration.getTopicName());
			int PartitionIdx = TopicName.getPartitionIndex(ReaderConfiguration.TopicName);
			consumer = new ConsumerImpl<T>(Client, ReaderConfiguration.TopicName, ConsumerConfiguration, ListenerExecutor, PartitionIdx, false, ConsumerFuture, SubscriptionMode.NonDurable, ReaderConfiguration.StartMessageId, ReaderConfiguration.StartMessageFromRollbackDurationInSec, Schema, null, true);
		}

		public class MessageListenerAnonymousInnerClass : MessageListener<T>
		{
			private readonly ReaderImpl<T> outerInstance;

			private ReaderListener<T> readerListener;

			public MessageListenerAnonymousInnerClass(ReaderImpl<T> OuterInstance, ReaderListener<T> ReaderListener)
			{
				this.outerInstance = OuterInstance;
				this.readerListener = ReaderListener;
				serialVersionUID = 1L;
			}

			private static readonly long serialVersionUID;

			public void received(IConsumer<T> Consumer, Message<T> Msg)
			{
				readerListener.Received(outerInstance, Msg);
				Consumer.acknowledgeCumulativeAsync(Msg);
			}

			public void reachedEndOfTopic(IConsumer<T> Consumer)
			{
				readerListener.ReachedEndOfTopic(outerInstance);
			}
		}

		public virtual string Topic
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

		public override bool HasReachedEndOfTopic()
		{
			return consumer.HasReachedEndOfTopic();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public Message<T> readNext() throws PulsarClientException
		public override Message<T> ReadNext()
		{
			Message<T> Msg = consumer.Receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			consumer.AcknowledgeCumulativeAsync(Msg);
			return Msg;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public Message<T> readNext(int timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws PulsarClientException
		public override Message<T> ReadNext(int Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Message<T> Msg = consumer.Receive(Timeout, Unit);

			if (Msg != null)
			{
				consumer.AcknowledgeCumulativeAsync(Msg);
			}
			return Msg;
		}

		public override CompletableFuture<Message<T>> ReadNextAsync()
		{
			return consumer.ReceiveAsync().thenApply(msg =>
			{
			consumer.AcknowledgeCumulativeAsync(msg);
			return msg;
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			consumer.Close();
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			return consumer.CloseAsync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean hasMessageAvailable() throws PulsarClientException
		public override bool HasMessageAvailable()
		{
			return consumer.HasMessageAvailable();
		}

		public override CompletableFuture<bool> HasMessageAvailableAsync()
		{
			return consumer.HasMessageAvailableAsync();
		}

		public virtual bool Connected
		{
			get
			{
				return consumer.Connected;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(MessageId messageId) throws PulsarClientException
		public override void Seek(IMessageId MessageId)
		{
			consumer.Seek(MessageId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws PulsarClientException
		public override void Seek(long Timestamp)
		{
			consumer.Seek(Timestamp);
		}

		public override CompletableFuture<Void> SeekAsync(IMessageId MessageId)
		{
			return consumer.SeekAsync(MessageId);
		}

		public override CompletableFuture<Void> SeekAsync(long Timestamp)
		{
			return consumer.SeekAsync(Timestamp);
		}
	}

}