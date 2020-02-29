
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

using System.Collections.Generic;
using System.Threading;

namespace SharpPulsar.Impl
{

	public class UnAckedTopicMessageTracker : UnAckedMessageTracker
    {
        private ReaderWriterLockSlim _readerWriterLock;
		public UnAckedTopicMessageTracker(PulsarClientImpl client, ConsumerBase consumerBase, long ackTimeoutMillis) : base(client, consumerBase, ackTimeoutMillis)
		{
			_readerWriterLock = new ReaderWriterLockSlim();
		}

		public UnAckedTopicMessageTracker(PulsarClientImpl client, ConsumerBase consumerBase, long ackTimeoutMillis, long tickDurationMillis) : base(client, consumerBase, ackTimeoutMillis, tickDurationMillis)
		{
            _readerWriterLock = new ReaderWriterLockSlim();
		}

		public virtual int RemoveTopicMessages(string topicName)
		{
            _readerWriterLock.EnterWriteLock();
			try
			{
				var removed = 0;
                using var iterator = MessageIdPartitionMap.Keys.GetEnumerator();
				while (iterator.MoveNext())
				{
					var messageId = iterator.Current;
					if (messageId is TopicMessageIdImpl impl && impl.TopicPartitionName.Contains(topicName))
					{
						var exist = MessageIdPartitionMap[impl];
                        exist?.TryRemove(impl);
                        MessageIdPartitionMap.Remove(impl, out var m);
						removed++;
					}
				}
				return removed;
			}
			finally
			{
                _readerWriterLock.ExitWriteLock();
			}
		}

	}

}