using System;
using System.Collections.Concurrent;
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
namespace SharpPulsar.Impl
{
	using Preconditions = com.google.common.@base.Preconditions;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;

	using MessageId = org.apache.pulsar.client.api.MessageId;
	using ConcurrentOpenHashSet = org.apache.pulsar.common.util.collections.ConcurrentOpenHashSet;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class UnAckedMessageTracker : System.IDisposable
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(UnAckedMessageTracker));

		protected internal readonly ConcurrentDictionary<MessageId, ConcurrentOpenHashSet<MessageId>> messageIdPartitionMap;
		protected internal readonly LinkedList<ConcurrentOpenHashSet<MessageId>> timePartitions;

		protected internal readonly Lock readLock;
		protected internal readonly Lock writeLock;

		public static readonly UnAckedMessageTrackerDisabled UNACKED_MESSAGE_TRACKER_DISABLED = new UnAckedMessageTrackerDisabled();
		private readonly long ackTimeoutMillis;
		private readonly long tickDurationInMs;

		private class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
		{
			public override void clear()
			{
			}

			internal override long size()
			{
				return 0;
			}

			public override bool add(MessageId m)
			{
				return true;
			}

			public override bool remove(MessageId m)
			{
				return true;
			}

			public override int removeMessagesTill(MessageId msgId)
			{
				return 0;
			}

			public virtual void Dispose()
			{
			}
		}

		private Timeout timeout;

		public UnAckedMessageTracker()
		{
			readLock = null;
			writeLock = null;
			timePartitions = null;
			messageIdPartitionMap = null;
			this.ackTimeoutMillis = 0;
			this.tickDurationInMs = 0;
		}

		public UnAckedMessageTracker<T1>(PulsarClientImpl client, ConsumerBase<T1> consumerBase, long ackTimeoutMillis) : this(client, consumerBase, ackTimeoutMillis, ackTimeoutMillis)
		{
		}

		private static readonly FastThreadLocal<HashSet<MessageId>> TL_MESSAGE_IDS_SET = new FastThreadLocalAnonymousInnerClass();

		private class FastThreadLocalAnonymousInnerClass : FastThreadLocal<HashSet<MessageId>>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected java.util.HashSet<org.apache.pulsar.client.api.MessageId> initialValue() throws Exception
			protected internal override HashSet<MessageId> initialValue()
			{
				return new HashSet<MessageId>();
			}
		}

		public UnAckedMessageTracker<T1>(PulsarClientImpl client, ConsumerBase<T1> consumerBase, long ackTimeoutMillis, long tickDurationInMs)
		{
			Preconditions.checkArgument(tickDurationInMs > 0 && ackTimeoutMillis >= tickDurationInMs);
			this.ackTimeoutMillis = ackTimeoutMillis;
			this.tickDurationInMs = tickDurationInMs;
			ReentrantReadWriteLock readWriteLock = new ReentrantReadWriteLock();
			this.readLock = readWriteLock.readLock();
			this.writeLock = readWriteLock.writeLock();
			this.messageIdPartitionMap = new ConcurrentDictionary<MessageId, ConcurrentOpenHashSet<MessageId>>();
			this.timePartitions = new LinkedList<ConcurrentOpenHashSet<MessageId>>();

			int blankPartitions = (int)Math.Ceiling((double)this.ackTimeoutMillis / this.tickDurationInMs);
			for (int i = 0; i < blankPartitions + 1; i++)
			{
				timePartitions.AddLast(new ConcurrentOpenHashSet<>(16, 1));
			}

			timeout = client.timer().newTimeout(new TimerTaskAnonymousInnerClass(this, client, consumerBase, tickDurationInMs)
		   , this.tickDurationInMs, TimeUnit.MILLISECONDS);
		}

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
			private readonly UnAckedMessageTracker outerInstance;

			private SharpPulsar.Impl.PulsarClientImpl client;
			private SharpPulsar.Impl.ConsumerBase<T1> consumerBase;
			private long tickDurationInMs;

			public TimerTaskAnonymousInnerClass(UnAckedMessageTracker outerInstance, SharpPulsar.Impl.PulsarClientImpl client, SharpPulsar.Impl.ConsumerBase<T1> consumerBase, long tickDurationInMs)
			{
				this.outerInstance = outerInstance;
				this.client = client;
				this.consumerBase = consumerBase;
				this.tickDurationInMs = tickDurationInMs;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout t) throws Exception
			public override void run(Timeout t)
			{
				ISet<MessageId> messageIds = TL_MESSAGE_IDS_SET.get();
				messageIds.Clear();

				outerInstance.writeLock.@lock();
				try
				{
					ConcurrentOpenHashSet<MessageId> headPartition = outerInstance.timePartitions.RemoveFirst();
					if (!headPartition.Empty)
					{
						log.warn("[{}] {} messages have timed-out", consumerBase, headPartition.size());
						headPartition.forEach(messageId =>
						{
						messageIds.Add(messageId);
						outerInstance.messageIdPartitionMap.Remove(messageId);
						});
					}

					headPartition.clear();
					outerInstance.timePartitions.AddLast(headPartition);
				}
				finally
				{
					if (messageIds.Count > 0)
					{
						consumerBase.onAckTimeoutSend(messageIds);
						consumerBase.redeliverUnacknowledgedMessages(messageIds);
					}
					outerInstance.timeout = client.timer().newTimeout(this, tickDurationInMs, TimeUnit.MILLISECONDS);
					outerInstance.writeLock.unlock();
				}
			}
		}

		public virtual void clear()
		{
			writeLock.@lock();
			try
			{
				messageIdPartitionMap.Clear();
				timePartitions.forEach(tp => tp.clear());
			}
			finally
			{
				writeLock.unlock();
			}
		}

		public virtual bool add(MessageId messageId)
		{
			writeLock.@lock();
			try
			{
				ConcurrentOpenHashSet<MessageId> partition = timePartitions.peekLast();
				ConcurrentOpenHashSet<MessageId> previousPartition = messageIdPartitionMap.GetOrAdd(messageId, partition);
				if (previousPartition == null)
				{
					return partition.add(messageId);
				}
				else
				{
					return false;
				}
			}
			finally
			{
				writeLock.unlock();
			}
		}

		internal virtual bool Empty
		{
			get
			{
				readLock.@lock();
				try
				{
					return messageIdPartitionMap.IsEmpty;
				}
				finally
				{
					readLock.unlock();
				}
			}
		}

		public virtual bool remove(MessageId messageId)
		{
			writeLock.@lock();
			try
			{
				bool removed = false;
				ConcurrentOpenHashSet<MessageId> exist = messageIdPartitionMap.Remove(messageId);
				if (exist != null)
				{
					removed = exist.remove(messageId);
				}
				return removed;
			}
			finally
			{
				writeLock.unlock();
			}
		}

		internal virtual long size()
		{
			readLock.@lock();
			try
			{
				return messageIdPartitionMap.Count;
			}
			finally
			{
				readLock.unlock();
			}
		}

		public virtual int removeMessagesTill(MessageId msgId)
		{
			writeLock.@lock();
			try
			{
				int removed = 0;
				IEnumerator<MessageId> iterator = messageIdPartitionMap.Keys.GetEnumerator();
				while (iterator.MoveNext())
				{
					MessageId messageId = iterator.Current;
					if (messageId.compareTo(msgId) <= 0)
					{
						ConcurrentOpenHashSet<MessageId> exist = messageIdPartitionMap[messageId];
						if (exist != null)
						{
							exist.remove(messageId);
						}
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						iterator.remove();
						removed++;
					}
				}
				return removed;
			}
			finally
			{
				writeLock.unlock();
			}
		}

		private void stop()
		{
			writeLock.@lock();
			try
			{
				if (timeout != null && !timeout.Cancelled)
				{
					timeout.cancel();
				}
				this.clear();
			}
			finally
			{
				writeLock.unlock();
			}
		}

		public virtual void Dispose()
		{
			stop();
		}
	}

}