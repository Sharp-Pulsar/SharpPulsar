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
using SharpPulsar.Utility;
using SharpPulsar.Utils;

namespace SharpPulsar.Impl
{

    public class ReaderImpl : IReader
	{

		private readonly ConsumerImpl _consumer;

		public ReaderImpl(PulsarClientImpl client, ReaderConfigurationData readerConfiguration, TaskCompletionSource<IConsumer> consumerTask, ISchema schema, ScheduledThreadPoolExecutor executor)
		{

			var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
			if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			var consumerConfiguration = new ConsumerConfigurationData();
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
			_consumer = new ConsumerImpl(client, readerConfiguration.TopicName, consumerConfiguration, executor, partitionIdx, false, consumerTask, SubscriptionMode.SubscriptionMode.NonDurable, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, null, true);
		}

		public class MessageListenerAnonymousInnerClass : IMessageListener
		{
			private readonly ReaderImpl _outerInstance;

			private IReaderListener _readerListener;

			public MessageListenerAnonymousInnerClass(ReaderImpl outerInstance, IReaderListener readerListener)
			{
				this._outerInstance = outerInstance;
				this._readerListener = readerListener;
				SerialVersionUid = 1L;
			}

			private static long SerialVersionUid;

			public void Received(IConsumer consumer, IMessage msg)
			{
				_readerListener.Received(_outerInstance, msg);
				consumer.AcknowledgeCumulativeAsync(msg);
			}

			public void ReachedEndOfTopic(IConsumer consumer)
			{
				_readerListener.ReachedEndOfTopic(_outerInstance);
			}
		}

		public virtual string Topic => _consumer.Topic;

        public virtual ConsumerImpl Consumer => _consumer;

        public bool HasReachedEndOfTopic()
		{
			return _consumer.HasReachedEndOfTopic();
		}

		public IMessage ReadNext()
		{
			var msg = _consumer.Receive();

			// Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
			// it will specify the subscription position anyway
			_consumer.AcknowledgeCumulativeAsync(msg);
			return msg;
		}

		public IMessage ReadNext(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			var msg = _consumer.Receive(timeout, unit);

			if (msg != null)
			{
				_consumer.AcknowledgeCumulativeAsync(msg);
			}
			return msg;
		}

		public ValueTask<IMessage> ReadNextAsync()
		{
			var r = _consumer.ReceiveAsync().AsTask().ContinueWith(task =>
            {
                var msg = task.Result;
			    _consumer.AcknowledgeCumulativeAsync(msg);
			    return msg;
			});
			return new ValueTask<IMessage>(r.Result);
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