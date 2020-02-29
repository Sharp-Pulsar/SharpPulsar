using SharpPulsar.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConcurrentCollections;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;

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

	public class UnAckedMessageTracker : IDisposable
    {
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(UnAckedMessageTracker));

		protected internal readonly ConcurrentDictionary<IMessageId, ConcurrentHashSet<IMessageId>> MessageIdPartitionMap;
		protected internal readonly LinkedList<ConcurrentHashSet<IMessageId>> TimePartitions;

		protected internal readonly ReaderWriterLockSlim ReadWriteLock;

		public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled = new UnAckedMessageTrackerDisabled();
		private readonly long _ackTimeoutMillis;
		private readonly long _tickDurationInMs;

		public class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
		{
			public override void Clear()
			{
			}

			public override long Size()
			{
				return 0;
			}

			public override bool Add(IMessageId m)
			{
				return true;
			}

			public override bool Remove(IMessageId m)
			{
				return true;
			}

			public override int RemoveMessagesTill(IMessageId msgId)
			{
				return 0;
			}

			public  void Close()
			{
			}
		}

		private ITimeout _timeout;

		public UnAckedMessageTracker()
        {
            ReadWriteLock = null;
			TimePartitions = null;
			MessageIdPartitionMap = null;
			this._ackTimeoutMillis = 0;
			this._tickDurationInMs = 0;
		}

		public UnAckedMessageTracker(PulsarClientImpl client, ConsumerBase consumerBase, long ackTimeoutMillis) : this(client, consumerBase, ackTimeoutMillis, ackTimeoutMillis)
		{
				
		}

		private static readonly FastThreadLocal<HashSet<IMessageId>> TlMessageIdsSet = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<HashSet<IMessageId>>
		{
			public  HashSet<IMessageId> initialValue()
			{
				return new HashSet<IMessageId>();
			}
		}

		public UnAckedMessageTracker(PulsarClientImpl client, ConsumerBase consumerBase, long ackTimeoutMillis, long tickDurationInMs)
		{
			if(tickDurationInMs == 0 && ackTimeoutMillis < tickDurationInMs) 
				throw new ArgumentException();
            this._ackTimeoutMillis = ackTimeoutMillis;
			this._tickDurationInMs = tickDurationInMs;
            ReadWriteLock = new ReaderWriterLockSlim();
			this.MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, ConcurrentHashSet<IMessageId>>();
			this.TimePartitions = new LinkedList<ConcurrentHashSet<IMessageId>>();

			var blankPartitions = (int)Math.Ceiling((double)this._ackTimeoutMillis / this._tickDurationInMs);
			for (var i = 0; i < blankPartitions + 1; i++)
			{
				TimePartitions.AddLast(new ConcurrentHashSet<IMessageId>(1, 16));
			}

			_timeout = client.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this, client, consumerBase, tickDurationInMs)
		   , TimeSpan.FromMilliseconds(_tickDurationInMs));
		}

		public class TimerTaskAnonymousInnerClass : ITimerTask
		{
			private readonly UnAckedMessageTracker _outerInstance;

			private PulsarClientImpl _client;
			private ConsumerBase _consumerBase;
			private long _tickDurationInMs;

			public TimerTaskAnonymousInnerClass(UnAckedMessageTracker outerInstance, PulsarClientImpl client, ConsumerBase consumerBase, long tickDurationInMs)
			{
				this._outerInstance = outerInstance;
				this._client = client;
				this._consumerBase = consumerBase;
				this._tickDurationInMs = tickDurationInMs;
			}
			public  void Run(ITimeout T)
			{
				ISet<IMessageId> messageIds = TlMessageIdsSet.Value;
				messageIds.Clear();

				_outerInstance.ReadWriteLock.EnterWriteLock();
				try
				{
					var headPartition = _outerInstance.TimePartitions.First.Value;
					if (!headPartition.IsEmpty)
                    {
                        _outerInstance.TimePartitions.Remove(headPartition);
						Log.LogWarning("[{}] {} messages have timed-out", _consumerBase, headPartition.Count);
						headPartition.ToList().ForEach(x =>
						{
                            messageIds.ToList().ForEach(x=>
                            {
                                if (x == null) throw new ArgumentNullException(nameof(x));
                                messageIds.Add(x);
                            });
                            messageIds.ToList().ForEach(x =>
                            {
                                if (x == null) throw new ArgumentNullException(nameof(x));
                                _outerInstance.MessageIdPartitionMap.TryRemove(x, out var m);
                            });
							
						});
					}

					headPartition.Clear();
					_outerInstance.TimePartitions.AddLast(headPartition);
				}
				finally
				{
					if (messageIds.Count > 0)
					{
						_consumerBase.OnAckTimeoutSend(messageIds);
						_consumerBase.RedeliverUnacknowledgedMessages(messageIds);
					}
					_outerInstance._timeout = _client.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(_tickDurationInMs));
					_outerInstance.ReadWriteLock.ExitWriteLock();
				}
			}
		}

		public virtual void Clear()
		{
			ReadWriteLock.EnterWriteLock();
			try
			{
				MessageIdPartitionMap.Clear();
				TimePartitions.ToList().ForEach(tp => tp.Clear());
			}
			finally
			{
				ReadWriteLock.ExitWriteLock();
			}
		}

		public virtual bool Add(IMessageId messageId)
		{
			ReadWriteLock.EnterWriteLock();
			try
			{
				var partition = TimePartitions.Last.Value;
				var previousPartition = MessageIdPartitionMap.GetOrAdd(messageId, partition);
				return previousPartition == null && partition.Add(messageId);
			}
			finally
			{
				ReadWriteLock.ExitWriteLock();
			}
		}

		public virtual bool Empty
		{
			get
			{
				ReadWriteLock.EnterReadLock();
				try
				{
					return MessageIdPartitionMap.IsEmpty;
				}
				finally
				{
					ReadWriteLock.ExitReadLock();
				}
			}
		}

		public virtual bool Remove(IMessageId messageId)
		{
			ReadWriteLock.EnterWriteLock();
			try
			{
				var removed = false;
				if (MessageIdPartitionMap.Remove(messageId, out var exist))
				{
					removed = exist.TryRemove(messageId);
				}
				return removed;
			}
			finally
			{
				ReadWriteLock.EnterWriteLock();
			}
		}

		public virtual long Size()
		{
			ReadWriteLock.EnterReadLock();
			try
			{
				return MessageIdPartitionMap.Count;
			}
			finally
			{
				ReadWriteLock.ExitReadLock();
			}
		}

		public virtual int RemoveMessagesTill(IMessageId msgId)
		{
			ReadWriteLock.EnterWriteLock();
			try
			{
				var removed = 0;
                using var iterator = MessageIdPartitionMap.Keys.GetEnumerator();
				while (iterator.MoveNext())
				{
					var messageId = iterator.Current;
					if (messageId != null && messageId.CompareTo(msgId) <= 0)
					{
						var exist = MessageIdPartitionMap[messageId];
                        exist?.TryRemove(messageId);
                        MessageIdPartitionMap.Remove(messageId, out var m);
						removed++;
					}
				}
				return removed;
			}
			finally
			{
				ReadWriteLock.ExitWriteLock();
			}
		}

		private void Stop()
		{
            ReadWriteLock.EnterWriteLock();
			try
			{
				if (_timeout != null && !_timeout.Canceled)
				{
					_timeout.Cancel();
				}
				this.Clear();
			}
			finally
			{
				ReadWriteLock.ExitWriteLock();
			}
		}

		public  void Close()
		{
			Stop();
		}

        public void Dispose()
        {
            Close();
        }
    }

}