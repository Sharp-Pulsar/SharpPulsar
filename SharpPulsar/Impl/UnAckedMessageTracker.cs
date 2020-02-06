using SharpPulsar.Api;
using SharpPulsar.Impl;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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

	public class UnAckedMessageTracker<T> : IDisposable
    {
		private static readonly Logger log = LoggerFactory.getLogger(typeof(UnAckedMessageTracker));

		protected internal readonly ConcurrentDictionary<IMessageId, ConcurrentOpenHashSet<IMessageId>> MessageIdPartitionMap;
		protected internal readonly LinkedList<ConcurrentOpenHashSet<IMessageId>> TimePartitions;

		protected internal readonly Lock ReadLock;
		protected internal readonly Lock WriteLock;

		public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled = new UnAckedMessageTrackerDisabled();
		private readonly long ackTimeoutMillis;
		private readonly long tickDurationInMs;

		public class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
		{
			public  void Clear()
			{
			}

			public  long Size()
			{
				return 0;
			}

			public  bool Add(IMessageId M)
			{
				return true;
			}

			public  bool Remove(IMessageId M)
			{
				return true;
			}

			public  int RemoveMessagesTill(IMessageId MsgId)
			{
				return 0;
			}

			public  void Close()
			{
			}
		}

		private readonly Timeout timeout;

		public UnAckedMessageTracker()
		{
			ReadLock = null;
			WriteLock = null;
			TimePartitions = null;
			MessageIdPartitionMap = null;
			this.ackTimeoutMillis = 0;
			this.tickDurationInMs = 0;
		}

		public UnAckedMessageTracker(PulsarClientImpl Client, ConsumerBase<T> ConsumerBase, long AckTimeoutMillis) : this(Client, ConsumerBase, AckTimeoutMillis, AckTimeoutMillis)
		{
				public void Dispose()
				{
					throw new NotImplementedException();
				}
		}

		private static readonly FastThreadLocal<HashSet<IMessageId>> TL_MESSAGE_IDS_SET = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<HashSet<IMessageId>>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @ protected java.util.HashSet<SharpPulsar.api.MessageId> initialValue() throws Exception
			public  HashSet<IMessageId> initialValue()
			{
				return new HashSet<IMessageId>();
			}
		}

		public UnAckedMessageTracker<T1>(PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, long AckTimeoutMillis, long TickDurationInMs)
		{
			Preconditions.checkArgument(TickDurationInMs > 0 && AckTimeoutMillis >= TickDurationInMs);
			this.ackTimeoutMillis = AckTimeoutMillis;
			this.tickDurationInMs = TickDurationInMs;
			ReentrantReadWriteLock ReadWriteLock = new ReentrantReadWriteLock();
			this.ReadLock = ReadWriteLock.readLock();
			this.WriteLock = ReadWriteLock.writeLock();
			this.MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, ConcurrentOpenHashSet<IMessageId>>();
			this.TimePartitions = new LinkedList<ConcurrentOpenHashSet<IMessageId>>();

			int BlankPartitions = (int)Math.Ceiling((double)this.ackTimeoutMillis / this.tickDurationInMs);
			for (int I = 0; I < BlankPartitions + 1; I++)
			{
				TimePartitions.AddLast(new ConcurrentOpenHashSet<>(16, 1));
			}

			timeout = Client.timer().newTimeout(new TimerTaskAnonymousInnerClass(this, Client, ConsumerBase, TickDurationInMs)
		   , this.tickDurationInMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		public class TimerTaskAnonymousInnerClass : TimerTask
		{
			private readonly UnAckedMessageTracker outerInstance;

			private PulsarClientImpl client;
			private ConsumerBase<T1> consumerBase;
			private long tickDurationInMs;

			public TimerTaskAnonymousInnerClass(UnAckedMessageTracker OuterInstance, PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, long TickDurationInMs)
			{
				this.outerInstance = OuterInstance;
				this.client = Client;
				this.consumerBase = ConsumerBase;
				this.tickDurationInMs = TickDurationInMs;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @ public void run(io.netty.util.Timeout t) throws Exception
			public  void run(Timeout T)
			{
				ISet<IMessageId> MessageIds = TL_MESSAGE_IDS_SET.get();
				MessageIds.Clear();

				outerInstance.WriteLock.@lock();
				try
				{
					ConcurrentOpenHashSet<IMessageId> HeadPartition = outerInstance.TimePartitions.RemoveFirst();
					if (!HeadPartition.Empty)
					{
						log.warn("[{}] {} messages have timed-out", consumerBase, HeadPartition.size());
						HeadPartition.forEach(messageId =>
						{
						MessageIds.Add(messageId);
						outerInstance.MessageIdPartitionMap.Remove(messageId);
						});
					}

					HeadPartition.clear();
					outerInstance.TimePartitions.AddLast(HeadPartition);
				}
				finally
				{
					if (MessageIds.Count > 0)
					{
						consumerBase.OnAckTimeoutSend(MessageIds);
						consumerBase.RedeliverUnacknowledgedMessages(MessageIds);
					}
					outerInstance.timeout = client.Timer().newTimeout(this, tickDurationInMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
					outerInstance.WriteLock.unlock();
				}
			}
		}

		public virtual void Clear()
		{
			WriteLock.@lock();
			try
			{
				MessageIdPartitionMap.Clear();
				TimePartitions.forEach(tp => tp.clear());
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public virtual bool Add(IMessageId MessageId)
		{
			WriteLock.@lock();
			try
			{
				ConcurrentOpenHashSet<IMessageId> Partition = TimePartitions.peekLast();
				ConcurrentOpenHashSet<IMessageId> PreviousPartition = MessageIdPartitionMap.GetOrAdd(MessageId, Partition);
				if (PreviousPartition == null)
				{
					return Partition.add(MessageId);
				}
				else
				{
					return false;
				}
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public virtual bool Empty
		{
			get
			{
				ReadLock.@lock();
				try
				{
					return MessageIdPartitionMap.IsEmpty;
				}
				finally
				{
					ReadLock.unlock();
				}
			}
		}

		public virtual bool Remove(IMessageId MessageId)
		{
			WriteLock.@lock();
			try
			{
				bool Removed = false;
				ConcurrentOpenHashSet<IMessageId> Exist = MessageIdPartitionMap.Remove(MessageId);
				if (Exist != null)
				{
					Removed = Exist.remove(MessageId);
				}
				return Removed;
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public virtual long Size()
		{
			ReadLock.@lock();
			try
			{
				return MessageIdPartitionMap.Count;
			}
			finally
			{
				ReadLock.unlock();
			}
		}

		public virtual int RemoveMessagesTill(IMessageId MsgId)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<IMessageId> Iterator = MessageIdPartitionMap.Keys.GetEnumerator();
				while (Iterator.MoveNext())
				{
					IMessageId MessageId = Iterator.Current;
					if (MessageId.CompareTo(MsgId) <= 0)
					{
						ConcurrentOpenHashSet<IMessageId> Exist = MessageIdPartitionMap[MessageId];
						if (Exist != null)
						{
							Exist.remove(MessageId);
						}
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						Iterator.remove();
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

		private void Stop()
		{
			WriteLock.@lock();
			try
			{
				if (timeout != null && !timeout.Cancelled)
				{
					timeout.cancel();
				}
				this.Clear();
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public  void Close()
		{
			Stop();
		}
	}

}