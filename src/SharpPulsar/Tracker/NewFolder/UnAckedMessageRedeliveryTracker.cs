using System;
using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Impl
{
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using RedeliveryBackoff = Org.Apache.Pulsar.Client.Api.RedeliveryBackoff;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class UnAckedMessageRedeliveryTracker : UnAckedMessageTracker
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(UnAckedMessageRedeliveryTracker));

		protected internal readonly Dictionary<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>> RedeliveryMessageIdPartitionMap;
		protected internal readonly LinkedList<HashSet<UnackMessageIdWrapper>> RedeliveryTimePartitions;

		protected internal readonly Dictionary<MessageId, long> AckTimeoutMessages;
		private readonly RedeliveryBackoff ackTimeoutRedeliveryBackoff;

// JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
// ORIGINAL LINE: public UnAckedMessageRedeliveryTracker(PulsarClientImpl client, ConsumerBase<?> consumerBase, org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf)
		public UnAckedMessageRedeliveryTracker(PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, ConsumerConfigurationData<T2> Conf) : base(Client, ConsumerBase, Conf)
		{
			this.ackTimeoutRedeliveryBackoff = Conf.getAckTimeoutRedeliveryBackoff();
			this.AckTimeoutMessages = new Dictionary<MessageId, long>();
			this.RedeliveryMessageIdPartitionMap = new Dictionary<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>>();
			this.RedeliveryTimePartitions = new LinkedList<HashSet<UnackMessageIdWrapper>>();

			int BlankPartitions = (int) Math.Ceiling((double) this.AckTimeoutMillis / this.TickDurationInMs);
			for (int I = 0; I < BlankPartitions + 1; I++)
			{
				RedeliveryTimePartitions.AddLast(new HashSet<>(16, 1));
			}

			Timeout = Client.Timer().newTimeout(new TimerTaskAnonymousInnerClass(this, Client, ConsumerBase)
		   , this.TickDurationInMs, TimeUnit.MILLISECONDS);

		}

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
			private readonly UnAckedMessageRedeliveryTracker outerInstance;

			private Org.Apache.Pulsar.Client.Impl.PulsarClientImpl client;
			private Org.Apache.Pulsar.Client.Impl.ConsumerBase<T1> consumerBase;

			public TimerTaskAnonymousInnerClass(UnAckedMessageRedeliveryTracker OuterInstance, Org.Apache.Pulsar.Client.Impl.PulsarClientImpl Client, Org.Apache.Pulsar.Client.Impl.ConsumerBase<T1> ConsumerBase)
			{
				this.outerInstance = OuterInstance;
				this.client = Client;
				this.consumerBase = ConsumerBase;
			}

// JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
// ORIGINAL LINE: @Override public void run(io.netty.util.Timeout t) throws Exception
			public override void run(Timeout T)
			{
				outerInstance.WriteLock.@lock();
				try
				{
					HashSet<UnackMessageIdWrapper> HeadPartition = outerInstance.RedeliveryTimePartitions.RemoveFirst();
					if (HeadPartition.Count > 0)
					{
						HeadPartition.forEach(unackMessageIdWrapper =>
						{
						outerInstance.AddAckTimeoutMessages(unackMessageIdWrapper);
						outerInstance.RedeliveryMessageIdPartitionMap.Remove(unackMessageIdWrapper);
						unackMessageIdWrapper.recycle();
						});
					}
					HeadPartition.Clear();
					outerInstance.RedeliveryTimePartitions.AddLast(HeadPartition);
					outerInstance.TriggerRedelivery(consumerBase);
				}
				finally
				{
					outerInstance.WriteLock.unlock();
					outerInstance.Timeout = client.Timer().newTimeout(this, outerInstance.TickDurationInMs, TimeUnit.MILLISECONDS);
				}
			}
		}

		private void AddAckTimeoutMessages(UnackMessageIdWrapper MessageIdWrapper)
		{
			WriteLock.@lock();
			try
			{
				MessageId MessageId = MessageIdWrapper.getMessageId();
				int RedeliveryCount = MessageIdWrapper.getRedeliveryCount();
				long BackoffNs = ackTimeoutRedeliveryBackoff.next(RedeliveryCount);
				AckTimeoutMessages[MessageId] = DateTimeHelper.CurrentUnixTimeMillis() + BackoffNs;
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		private void TriggerRedelivery<T1>(ConsumerBase<T1> ConsumerBase)
		{
			if (AckTimeoutMessages.Count == 0)
			{
				return;
			}
			ISet<MessageId> MessageIds = TlMessageIdsSet.get();
			MessageIds.Clear();

			try
			{
				long Now = DateTimeHelper.CurrentUnixTimeMillis();
				AckTimeoutMessages.forEach((messageId, timestamp) =>
				{
				if (timestamp <= Now)
				{
					AddChunkedMessageIdsAndRemoveFromSequenceMap(messageId, MessageIds, ConsumerBase);
					MessageIds.Add(messageId);
				}
				});
				if (MessageIds.Count > 0)
				{
					log.info("[{}] {} messages will be re-delivered", ConsumerBase, MessageIds.Count);
					IEnumerator<MessageId> Iterator = MessageIds.GetEnumerator();
					while (Iterator.MoveNext())
					{
						MessageId MessageId = Iterator.Current;
						AckTimeoutMessages.Remove(MessageId);
					}
				}
			}
			finally
			{
				if (MessageIds.Count > 0)
				{
					ConsumerBase.OnAckTimeoutSend(MessageIds);
					ConsumerBase.RedeliverUnacknowledgedMessages(MessageIds);
				}
			}
		}

		internal override bool Empty
		{
			get
			{
				ReadLock.@lock();
				try
				{
					return RedeliveryMessageIdPartitionMap.Count == 0 && AckTimeoutMessages.Count == 0;
				}
				finally
				{
					ReadLock.unlock();
				}
			}
		}

		public override void Clear()
		{
			WriteLock.@lock();
			try
			{
				RedeliveryMessageIdPartitionMap.Clear();
				RedeliveryTimePartitions.forEach(tp =>
				{
				tp.forEach(unackMessageIdWrapper => unackMessageIdWrapper.recycle());
				tp.clear();
				});
				AckTimeoutMessages.Clear();
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public override bool Add(MessageId MessageId)
		{
			return Add(MessageId, 0);
		}

		public override bool Add(MessageId MessageId, int RedeliveryCount)
		{
			WriteLock.@lock();
			try
			{
				UnackMessageIdWrapper MessageIdWrapper = UnackMessageIdWrapper.ValueOf(MessageId, RedeliveryCount);
				HashSet<UnackMessageIdWrapper> Partition = RedeliveryTimePartitions.peekLast();
				HashSet<UnackMessageIdWrapper> PreviousPartition = RedeliveryMessageIdPartitionMap.putIfAbsent(MessageIdWrapper, Partition);
				if (PreviousPartition == null)
				{
					return Partition.Add(MessageIdWrapper);
				}
				else
				{
					MessageIdWrapper.Recycle();
					return false;
				}
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public override bool Remove(MessageId MessageId)
		{
			WriteLock.@lock();
			UnackMessageIdWrapper MessageIdWrapper = UnackMessageIdWrapper.ValueOf(MessageId);
			try
			{
				bool Removed = false;
				HashSet<UnackMessageIdWrapper> Exist = RedeliveryMessageIdPartitionMap.Remove(MessageIdWrapper);
				if (Exist != null)
				{
					Removed = Exist.Remove(MessageIdWrapper);
				}
				return Removed || AckTimeoutMessages.Remove(MessageId) != null;
			}
			finally
			{
				MessageIdWrapper.Recycle();
				WriteLock.unlock();
			}
		}

		internal override long Size()
		{
			ReadLock.@lock();
			try
			{
				return RedeliveryMessageIdPartitionMap.Count + AckTimeoutMessages.Count;
			}
			finally
			{
				ReadLock.unlock();
			}
		}

		public override int RemoveMessagesTill(MessageId MsgId)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<KeyValuePair<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>>> Iterator = RedeliveryMessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
				while (Iterator.MoveNext())
				{
					KeyValuePair<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>> Entry = Iterator.Current;
					UnackMessageIdWrapper MessageIdWrapper = Entry.Key;
					if (MessageIdWrapper.getMessageId().compareTo(MsgId) <= 0)
					{
						Entry.Value.remove(MessageIdWrapper);
// JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						Iterator.remove();
						MessageIdWrapper.Recycle();
						Removed++;
					}
				}

				IEnumerator<MessageId> IteratorAckTimeOut = AckTimeoutMessages.Keys.GetEnumerator();
				while (IteratorAckTimeOut.MoveNext())
				{
					MessageId MessageId = IteratorAckTimeOut.Current;
					if (MessageId.compareTo(MsgId) <= 0)
					{
// JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						IteratorAckTimeOut.remove();
						Removed++;
					}
				}
				return Removed;
			}
			finally
			{
				WriteLock.unlock();
			}
		}

	}

}