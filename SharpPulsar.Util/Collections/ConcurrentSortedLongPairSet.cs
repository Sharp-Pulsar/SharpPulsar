using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Util.Collections
{

	using LongPair = Util.collections.ConcurrentLongPairSet.LongPair;
	using LongPairConsumer = Util.collections.ConcurrentLongPairSet.LongPairConsumer;

	/// <summary>
	/// Sorted concurrent <seealso cref="LongPairSet"/> which is not fully accurate in sorting.
	/// 
	/// <seealso cref="ConcurrentSortedLongPairSet"/> creates separate <seealso cref="ConcurrentLongPairSet"/> for unique first-key of
	/// inserted item. So, it can iterate over all items by sorting on item's first key. However, item's second key will not
	/// be sorted. eg:
	/// 
	/// <pre>
	///  insert: (1,2), (1,4), (2,1), (1,5), (2,6)
	///  while iterating set will first read all the entries for items whose first-key=1 and then first-key=2.
	///  output: (1,4), (1,5), (1,2), (2,6), (2,1)
	/// </pre>
	/// 
	/// <para>This map can be expensive and not recommended if set has to store large number of unique item.first's key
	/// because set has to create that many <seealso cref="ConcurrentLongPairSet"/> objects.
	/// </para>
	/// </summary>
	public class ConcurrentSortedLongPairSet : LongPairSet
	{

		protected internal readonly NavigableMap<long, ConcurrentLongPairSet> longPairSets = new ConcurrentSkipListMap<long, ConcurrentLongPairSet>();
		private int expectedItems;
		private int concurrencyLevel;
		/// <summary>
		/// If <seealso cref="longPairSets"/> adds and removes the item-set frequently then it allocates and removes
		/// <seealso cref="ConcurrentLongPairSet"/> for the same item multiple times which can lead to gc-puases. To avoid such
		/// situation, avoid removing empty LogPairSet until it reaches max limit.
		/// </summary>
		private int maxAllowedSetOnRemove;
		private const int DEFAULT_MAX_ALLOWED_SET_ON_REMOVE = 10;

		public ConcurrentSortedLongPairSet() : this(16, 1, DEFAULT_MAX_ALLOWED_SET_ON_REMOVE)
		{
		}

		public ConcurrentSortedLongPairSet(int expectedItems) : this(expectedItems, 1, DEFAULT_MAX_ALLOWED_SET_ON_REMOVE)
		{
		}

		public ConcurrentSortedLongPairSet(int expectedItems, int concurrencyLevel) : this(expectedItems, concurrencyLevel, DEFAULT_MAX_ALLOWED_SET_ON_REMOVE)
		{
		}

		public ConcurrentSortedLongPairSet(int expectedItems, int concurrencyLevel, int maxAllowedSetOnRemove)
		{
			this.expectedItems = expectedItems;
			this.concurrencyLevel = concurrencyLevel;
			this.maxAllowedSetOnRemove = maxAllowedSetOnRemove;
		}

		public virtual bool add(long item1, long item2)
		{
			ConcurrentLongPairSet messagesToReplay = longPairSets.computeIfAbsent(item1, (key) => new ConcurrentLongPairSet(expectedItems, concurrencyLevel));
			return messagesToReplay.add(item1, item2);
		}

		public virtual bool remove(long item1, long item2)
		{
			ConcurrentLongPairSet messagesToReplay = longPairSets.get(item1);
			if (messagesToReplay != null)
			{
				bool removed = messagesToReplay.remove(item1, item2);
				if (messagesToReplay.Empty && longPairSets.size() > maxAllowedSetOnRemove)
				{
					longPairSets.remove(item1, messagesToReplay);
				}
				return removed;
			}
			return false;
		}

		public virtual int removeIf(LongPairSet_LongPairPredicate filter)
		{
			AtomicInteger removedValues = new AtomicInteger(0);
			longPairSets.forEach((item1, longPairSet) =>
			{
			removedValues.addAndGet(longPairSet.removeIf(filter));
			if (longPairSet.Empty && longPairSets.size() > maxAllowedSetOnRemove)
			{
				longPairSets.remove(item1, longPairSet);
			}
			});
			return removedValues.get();
		}

		public virtual ISet<LongPair> items()
		{
			return items((int) this.size());
		}

		public virtual void forEach(LongPairConsumer processor)
		{
			foreach (long? item1 in longPairSets.navigableKeySet())
			{
				ConcurrentLongPairSet messagesToReplay = longPairSets.get(item1);
				messagesToReplay.forEach((i1, i2) =>
				{
				processor.accept(i1, i2);
				});
			}
		}

		public virtual ISet<LongPair> items(int numberOfItems)
		{
			return items(numberOfItems, (item1, item2) => new LongPair(item1, item2));
		}

		public virtual ISet<T> items<T>(int numberOfItems, LongPairSet_LongPairFunction<T> longPairConverter)
		{
			ISet<T> items = new SortedSet<T>();
			AtomicInteger count = new AtomicInteger(0);
			foreach (long? item1 in longPairSets.navigableKeySet())
			{
				if (count.get() >= numberOfItems)
				{ // already found set of positions
					break;
				}
				ConcurrentLongPairSet messagesToReplay = longPairSets.get(item1);
				messagesToReplay.forEach((i1, i2) =>
				{
				if (count.get() < numberOfItems)
				{
					items.Add(longPairConverter.apply(i1, i2));
					count.incrementAndGet();
				}
				});
			}
			return items;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean first = new java.util.concurrent.atomic.AtomicBoolean(true);
			AtomicBoolean first = new AtomicBoolean(true);
			longPairSets.forEach((key, longPairSet) =>
			{
			longPairSet.forEach((item1, item2) =>
			{
				if (!first.getAndSet(false))
				{
					sb.Append(", ");
				}
				sb.Append('[');
				sb.Append(item1);
				sb.Append(':');
				sb.Append(item2);
				sb.Append(']');
			});
			});
			sb.Append('}');
			return sb.ToString();
		}

		public virtual bool Empty
		{
			get
			{
				AtomicBoolean isEmpty = new AtomicBoolean(true);
				longPairSets.forEach((item1, longPairSet) =>
				{
				if (isEmpty.get() && !longPairSet.Empty)
				{
					isEmpty.set(false);
				}
				});
				return isEmpty.get();
			}
		}

		public virtual void clear()
		{
			longPairSets.clear();
		}

		public virtual long size()
		{
			AtomicLong size = new AtomicLong(0);
			longPairSets.forEach((item1, longPairSet) =>
			{
			size.getAndAdd(longPairSet.size());
			});
			return size.get();
		}

		public virtual bool contains(long item1, long item2)
		{
			ConcurrentLongPairSet longPairSet = longPairSets.get(item1);
			if (longPairSet != null)
			{
				return longPairSet.contains(item1, item2);
			}
			return false;
		}

	}
}