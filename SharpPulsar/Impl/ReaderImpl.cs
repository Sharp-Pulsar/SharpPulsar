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

using System;
using System.Threading.Tasks;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Util;
using SharpPulsar.Utils;

namespace SharpPulsar.Impl
{

    public class ReaderImpl<T> : IReader<T>
	{

		private readonly ConsumerImpl<T> _consumer;

		public ReaderImpl(PulsarClientImpl client, ReaderConfigurationData<T> readerConfiguration, TaskCompletionSource<IConsumer<T>> consumerTask, ISchema<T> schema, ScheduledThreadPoolExecutor executor)
		{

			var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
			if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			var consumerConfiguration = new ConsumerConfigurationData<T>();
			consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
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
				var readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(this, readerListener);
			}

			consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
			if (readerConfiguration.CryptoKeyReader != null)
			{
				consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
			}

			var partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
			_consumer = new ConsumerImpl<T>(client, readerConfiguration.TopicName, consumerConfiguration, executor, partitionIdx, false, consumerTask, ConsumerImpl<T>.SubscriptionMode.NonDurable, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, null, true);
		}

		public class MessageListenerAnonymousInnerClass : IMessageListener<T>
		{
			private readonly ReaderImpl<T> _outerInstance;

			private IReaderListener<T> _readerListener;

			public MessageListenerAnonymousInnerClass(ReaderImpl<T> outerInstance, IReaderListener<T> readerListener)
			{
				this._outerInstance = outerInstance;
				this._readerListener = readerListener;
				SerialVersionUid = 1L;
			}

			private static long SerialVersionUid;

			public void Received(IConsumer<T> consumer, IMessage<T> msg)
			{
				_readerListener.Received(_outerInstance, msg);
				consumer.AcknowledgeCumulativeAsync(msg);
			}

			public void ReachedEndOfTopic(IConsumer<T> consumer)
			{
				_readerListener.ReachedEndOfTopic(_outerInstance);
			}
		}

		public virtual string Topic => _consumer.Topic;

        public virtual ConsumerImpl<T> Consumer => _consumer;

        public bool HasReachedEndOfTopic()
		{
			return _consumer.HasReachedEndOfTopic();
		}

		public IMessage<T> ReadNext()
		{
			var msg = _consumer.Receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			_consumer.AcknowledgeCumulativeAsync(msg);
			return msg;
		}

		public IMessage<T> ReadNext(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			var msg = _consumer.Receive(timeout, unit);

			if (msg != null)
			{
				_consumer.AcknowledgeCumulativeAsync(msg);
			}
			return msg;
		}

		public ValueTask<IMessage<T>> ReadNextAsync()
		{
			var r = _consumer.ReceiveAsync().AsTask().ContinueWith(task =>
            {
                var msg = task.Result;
			    _consumer.AcknowledgeCumulativeAsync(msg);
			    return msg;
			});
			return new ValueTask<IMessage<T>>(r.Result);
		}

		public void Close()
		{
			_consumer.Close();
		}

		public ValueTask CloseAsync()
		{
			return _consumer.CloseAsync();
		}

		public bool HasMessageAvailable()
		{
			return _consumer.HasMessageAvailable();
		}

		public ValueTask<bool> HasMessageAvailableAsync()
		{
			return _consumer.HasMessageAvailableAsync();
		}

		public virtual bool Connected => _consumer.Connected;

		public void Seek(IMessageId messageId)
		{
			_consumer.Seek(messageId);
		}

		public void Seek(long timestamp)
		{
			_consumer.Seek(timestamp);
		}

		public ValueTask SeekAsync(IMessageId messageId)
		{
			return _consumer.SeekAsync(messageId);
		}

		public ValueTask SeekAsync(long timestamp)
		{
			return _consumer.SeekAsync(timestamp);
		}

        public void Dispose()
        {
            CloseAsync();
        }
    }

}