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
// JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
// 	import static com.google.common.@base.Preconditions.checkArgument;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class UnAckedMessageTracker : System.IDisposable
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(UnAckedMessageTracker));

		protected internal readonly Dictionary<MessageId, HashSet<MessageId>> MessageIdPartitionMap;
		protected internal readonly LinkedList<HashSet<MessageId>> TimePartitions;

		protected internal readonly Lock ReadLock;
		protected internal readonly Lock WriteLock;

		public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled = new UnAckedMessageTrackerDisabled();
		protected internal readonly long AckTimeoutMillis;
		protected internal readonly long TickDurationInMs;

		private class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
		{
			public override void Clear()
			{
			}

			internal override long Size()
			{
				return 0;
			}

			public override bool Add(MessageId M)
			{
				return true;
			}

			public override bool Add(MessageId MessageId, int RedeliveryCount)
			{
				return true;
			}

			public override bool Remove(MessageId M)
			{
				return true;
			}

			public override int RemoveMessagesTill(MessageId MsgId)
			{
				return 0;
			}

			public override void Dispose()
			{
			}
		}

		protected internal Timeout Timeout;

		public UnAckedMessageTracker()
		{
			ReadLock = null;
			WriteLock = null;
			TimePartitions = null;
			MessageIdPartitionMap = null;
			this.AckTimeoutMillis = 0;
			this.TickDurationInMs = 0;
		}

		protected internal static readonly FastThreadLocal<HashSet<MessageId>> TlMessageIdsSet = new FastThreadLocalAnonymousInnerClass();

		private class FastThreadLocalAnonymousInnerClass : FastThreadLocal<HashSet<MessageId>>
		{
// JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
// ORIGINAL LINE: @Override protected java.util.HashSet<org.apache.pulsar.client.api.MessageId> initialValue() throws Exception
			protected internal override HashSet<MessageId> initialValue()
			{
				return new HashSet<MessageId>();
			}
		}

// JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
// ORIGINAL LINE: public UnAckedMessageTracker(PulsarClientImpl client, ConsumerBase<?> consumerBase, org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf)
		public UnAckedMessageTracker(PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, ConsumerConfigurationData<T2> Conf)
		{
			this.AckTimeoutMillis = Conf.getAckTimeoutMillis();
			this.TickDurationInMs = Math.Min(Conf.getTickDurationMillis(), Conf.getAckTimeoutMillis());
			checkArgument(TickDurationInMs > 0 && AckTimeoutMillis >= TickDurationInMs);
			ReentrantReadWriteLock ReadWriteLock = new ReentrantReadWriteLock();
			this.ReadLock = ReadWriteLock.readLock();
			this.WriteLock = ReadWriteLock.writeLock();
			if (Conf.getAckTimeoutRedeliveryBackoff() == null)
			{
				this.MessageIdPartitionMap = new Dictionary<MessageId, HashSet<MessageId>>();
				this.TimePartitions = new LinkedList<HashSet<MessageId>>();

				int BlankPartitions = (int) Math.Ceiling((double) this.AckTimeoutMillis / this.TickDurationInMs);
				for (int I = 0; I < BlankPartitions + 1; I++)
				{
					TimePartitions.AddLast(new HashSet<>(16, 1));
				}
				Timeout = Client.Timer().newTimeout(new TimerTaskAnonymousInnerClass(this, Client, ConsumerBase)
			   , this.TickDurationInMs, TimeUnit.MILLISECONDS);
			}
			else
			{
				this.MessageIdPartitionMap = null;
				this.TimePartitions = null;
			}
		}

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
			private readonly UnAckedMessageTracker outerInstance;

			private Org.Apache.Pulsar.Client.Impl.PulsarClientImpl client;
			private Org.Apache.Pulsar.Client.Impl.ConsumerBase<T1> consumerBase;

			public TimerTaskAnonymousInnerClass(UnAckedMessageTracker OuterInstance, Org.Apache.Pulsar.Client.Impl.PulsarClientImpl Client, Org.Apache.Pulsar.Client.Impl.ConsumerBase<T1> ConsumerBase)
			{
				this.outerInstance = OuterInstance;
				this.client = Client;
				this.consumerBase = ConsumerBase;
			}

// JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
// ORIGINAL LINE: @Override public void run(io.netty.util.Timeout t) throws Exception
			public override void run(Timeout T)
			{
				if (T.isCancelled())
				{
					return;
				}

				ISet<MessageId> MessageIds = TL_MESSAGE_IDS_SET.get();
				MessageIds.Clear();

				outerInstance.WriteLock.@lock();
				try
				{
					HashSet<MessageId> HeadPartition = outerInstance.TimePartitions.RemoveFirst();
					if (HeadPartition.Count > 0)
					{
						log.info("[{}] {} messages will be re-delivered", consumerBase, HeadPartition.Count);
						HeadPartition.forEach(messageId =>
						{
						AddChunkedMessageIdsAndRemoveFromSequenceMap(messageId, MessageIds, consumerBase);
						MessageIds.Add(messageId);
						outerInstance.MessageIdPartitionMap.Remove(messageId);
						});
					}

					HeadPartition.Clear();
					outerInstance.TimePartitions.AddLast(HeadPartition);
				}
				finally
				{
					try
					{
						outerInstance.Timeout = client.Timer().newTimeout(this, outerInstance.TickDurationInMs, TimeUnit.MILLISECONDS);
					}
					finally
					{
						outerInstance.WriteLock.unlock();

						if (MessageIds.Count > 0)
						{
							consumerBase.OnAckTimeoutSend(MessageIds);
							consumerBase.RedeliverUnacknowledgedMessages(MessageIds);
						}
					}
				}
			}
		}

		public static void AddChunkedMessageIdsAndRemoveFromSequenceMap<T1>(MessageId MessageId, ISet<MessageId> MessageIds, ConsumerBase<T1> ConsumerBase)
		{
			if (MessageId is MessageIdImpl)
			{
				MessageIdImpl[] ChunkedMsgIds = ConsumerBase.UnAckedChunkedMessageIdSequenceMap.get((MessageIdImpl) MessageId);
				if (ChunkedMsgIds != null && ChunkedMsgIds.Length > 0)
				{
					Collections.addAll(MessageIds, ChunkedMsgIds);
				}
				ConsumerBase.UnAckedChunkedMessageIdSequenceMap.remove((MessageIdImpl) MessageId);
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

		public virtual bool Add(MessageId MessageId)
		{
			WriteLock.@lock();
			try
			{
				HashSet<MessageId> Partition = TimePartitions.peekLast();
				HashSet<MessageId> PreviousPartition = MessageIdPartitionMap.putIfAbsent(MessageId, Partition);
				if (PreviousPartition == null)
				{
					return Partition.Add(MessageId);
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

		public virtual bool Add(MessageId MessageId, int RedeliveryCount)
		{
			return Add(MessageId);
		}

		internal virtual bool Empty
		{
			get
			{
				ReadLock.@lock();
				try
				{
					return MessageIdPartitionMap.Count == 0;
				}
				finally
				{
					ReadLock.unlock();
				}
			}
		}

		public virtual bool Remove(MessageId MessageId)
		{
			WriteLock.@lock();
			try
			{
				bool Removed = false;
				HashSet<MessageId> Exist = MessageIdPartitionMap.Remove(MessageId);
				if (Exist != null)
				{
					Removed = Exist.Remove(MessageId);
				}
				return Removed;
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		internal virtual long Size()
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

		public virtual int RemoveMessagesTill(MessageId MsgId)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<KeyValuePair<MessageId, HashSet<MessageId>>> Iterator = MessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
				while (Iterator.MoveNext())
				{
					KeyValuePair<MessageId, HashSet<MessageId>> Entry = Iterator.Current;
					MessageId MessageId = Entry.Key;
					if (MessageId.compareTo(MsgId) <= 0)
					{
						Entry.Value.remove(MessageId);
// JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
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
				if (Timeout != null && !Timeout.isCancelled())
				{
					Timeout.cancel();
					Timeout = null;
				}
				this.Clear();
			}
			finally
			{
				WriteLock.unlock();
			}
		}

		public virtual void Dispose()
		{
			Stop();
		}
	}

}