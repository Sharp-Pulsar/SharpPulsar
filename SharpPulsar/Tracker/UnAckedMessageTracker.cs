
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Akka.Actor;
using DotNetty.Common;
using PulsarAdmin.Models;
using SharpPulsar.Impl;
using SharpPulsar.Utility;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker
    {

		protected internal readonly ConcurrentDictionary<MessageId, ConcurrentOpenHashSet<MessageId>> messageIdPartitionMap;
		protected internal readonly LinkedList<ConcurrentOpenHashSet<MessageId>> timePartitions;


		public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled = new UnAckedMessageTrackerDisabled();
		private readonly long ackTimeoutMillis;
		private readonly long tickDurationInMs;

		public class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
		{
			public override void Clear()
			{
			}

			public override long Size()
			{
				return 0;
			}

			public override bool Add(MessageId m)
			{
				return true;
			}

			public override bool Remove(MessageId m)
			{
				return true;
			}

			public override int RemoveMessagesTill(MessageId msgId)
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
			timePartitions = null;
			messageIdPartitionMap = null;
			this.ackTimeoutMillis = 0;
			this.tickDurationInMs = 0;
		}

		public UnAckedMessageTracker(IActorRef consumer, long ackTimeoutMillis) : this(ackTimeoutMillis, ackTimeoutMillis)
		{
		}

	private static readonly FastThreadLocal<HashSet<MessageId>> TlMessageIdsSet = new FastThreadLocalAnonymousInnerClass();

	public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<HashSet<MessageId>>
	{
		public override HashSet<MessageId> InitialValue()
		{
			return new HashSet<MessageId>();
		}
	}

	public UnAckedMessageTracker(IActorRef consumerBase, long ackTimeoutMillis, long tickDurationInMs)
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

	public class TimerTaskAnonymousInnerClass : TimerTask
	{
		private readonly UnAckedMessageTracker outerInstance;

		private PulsarClientImpl client;
		private ConsumerBase<T1> consumerBase;
		private long tickDurationInMs;

		public TimerTaskAnonymousInnerClass(UnAckedMessageTracker outerInstance, PulsarClientImpl client, ConsumerBase<T1> consumerBase, long tickDurationInMs)
		{
			this.outerInstance = outerInstance;
			this.client = client;
			this.consumerBase = consumerBase;
			this.tickDurationInMs = tickDurationInMs;
		}

		public override void Run(Timeout t)
		{
			ISet<MessageId> messageIds = TlMessageIdsSet.get();
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
						AddChunkedMessageIdsAndRemoveFromSequnceMap(messageId, messageIds, consumerBase);
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
				outerInstance.timeout = client.timer().newTimeout(this, tickDurationInMs, TimeUnit.ToMillis(BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS, ));
				outerInstance.writeLock.unlock();
			}
		}
	}

	public static void AddChunkedMessageIdsAndRemoveFromSequnceMap(MessageId messageId, ISet<MessageId> messageIds, IActorRef consumerBase)
	{
		if (messageId is MessageIdImpl)
		{
			MessageIdImpl[] chunkedMsgIds = consumerBase.unAckedChunckedMessageIdSequenceMap.get((MessageIdImpl)messageId);
			if (chunkedMsgIds != null && chunkedMsgIds.Length > 0)
			{
				foreach (MessageIdImpl msgId in chunkedMsgIds)
				{
					messageIds.Add(msgId);
				}
			}
			consumerBase.unAckedChunckedMessageIdSequenceMap.remove((MessageIdImpl)messageId);
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

	public virtual bool Add(MessageId messageId)
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

	public virtual bool Empty
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

	public virtual bool Remove(MessageId messageId)
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

	public virtual long Size()
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

	public virtual int RemoveMessagesTill(MessageId msgId)
	{
		writeLock.@lock();
		try
		{
			int removed = 0;
			IEnumerator<MessageId> iterator = messageIdPartitionMap.Keys.GetEnumerator();
			while (iterator.MoveNext())
			{
				MessageId messageId = iterator.Current;
				if (messageId.CompareTo(msgId) <= 0)
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

	private void Stop()
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

}
}
