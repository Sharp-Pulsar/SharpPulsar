using System;
using System.Collections;
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
namespace org.apache.pulsar.common.util.collections
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using BoundType = com.google.common.collect.BoundType;
	using Range = com.google.common.collect.Range;

	/// <summary>
	/// A Concurrent set comprising zero or more ranges of type <seealso cref="LongPair"/>. This can be alternative of
	/// <seealso cref="com.google.common.collect.RangeSet"/> and can be used if {@code range} type is <seealso cref="LongPair"/>
	/// 
	/// <pre>
	/// Usage:
	/// a. This can be used if one doesn't want to create object for every new inserted {@code range}
	/// b. It creates <seealso cref="System.Collections.BitArray"/> for every unique first-key of the range.
	/// So, this rangeSet is not suitable for large number of unique keys.
	/// </pre>
	/// </summary>
	public class ConcurrentOpenLongPairRangeSet<T> : LongPairRangeSet<T> where T : IComparable<T>
	{

		protected internal readonly NavigableMap<long, BitArray> rangeBitSetMap = new ConcurrentSkipListMap<long, BitArray>();
		private bool threadSafe = true;
		private readonly int bitSetSize;
		private readonly LongPairRangeSet_LongPairConsumer<T> consumer;

		// caching place-holder for cpu-optimization to avoid calculating ranges again
		private volatile int cachedSize = 0;
		private volatile string cachedToString = "[]";
		private volatile bool updatedAfterCachedForSize = true;
		private volatile bool updatedAfterCachedForToString = true;

		public ConcurrentOpenLongPairRangeSet(LongPairRangeSet_LongPairConsumer<T> consumer) : this(1024, true, consumer)
		{
		}

		public ConcurrentOpenLongPairRangeSet(int size, LongPairRangeSet_LongPairConsumer<T> consumer) : this(size, true, consumer)
		{
		}

		public ConcurrentOpenLongPairRangeSet(int size, bool threadSafe, LongPairRangeSet_LongPairConsumer<T> consumer)
		{
			this.threadSafe = threadSafe;
			this.bitSetSize = size;
			this.consumer = consumer;
		}

		/// <summary>
		/// Adds the specified range to this {@code RangeSet} (optional operation). That is, for equal range sets a and b,
		/// the result of {@code a.add(range)} is that {@code a} will be the minimal range set for which both
		/// {@code a.enclosesAll(b)} and {@code a.encloses(range)}.
		/// 
		/// <para>Note that {@code range} will merge given {@code range} with any ranges in the range set that are
		/// <seealso cref="Range.isConnected(Range) connected"/> with it. Moreover, if {@code range} is empty, this is a no-op.
		/// </para>
		/// </summary>
		public virtual void addOpenClosed(long lowerKey, long lowerValueOpen, long upperKey, long upperValue)
		{
			long lowerValue = lowerValueOpen + 1;
			if (lowerKey != upperKey)
			{
				// (1) set lower to last in lowerRange.getKey()
				if (isValid(lowerKey, lowerValue))
				{
					BitArray rangeBitSet = rangeBitSetMap.get(lowerKey);
					// if lower and upper has different key/ledger then set ranges for lower-key only if
					// a. bitSet already exist and given value is not the last value in the bitset.
					// it will prevent setting up values which are not actually expected to set
					// eg: (2:10..4:10] in this case , don't set any value for 2:10 and set [4:0..4:10]
					if (rangeBitSet != null && (rangeBitSet.previousSetBit(rangeBitSet.Count) > lowerValueOpen))
					{
						int lastValue = rangeBitSet.previousSetBit(rangeBitSet.Count);
						rangeBitSet.Set((int) lowerValue, (int) Math.Max(lastValue, lowerValue) + 1);
					}
				}
				// (2) set 0th-index to upper-index in upperRange.getKey()
				if (isValid(upperKey, upperValue))
				{
					BitArray rangeBitSet = rangeBitSetMap.computeIfAbsent(upperKey, (key) => createNewBitSet());
					if (rangeBitSet != null)
					{
						rangeBitSet.Set(0, (int) upperValue + 1);
					}
				}
				// No-op if values are not valid eg: if lower == LongPair.earliest or upper == LongPair.latest then nothing
				// to set
			}
			else
			{
				long key = lowerKey;
				BitArray rangeBitSet = rangeBitSetMap.computeIfAbsent(key, (k) => createNewBitSet());
				rangeBitSet.Set((int) lowerValue, (int) upperValue + 1);
			}
			updatedAfterCachedForSize = true;
			updatedAfterCachedForToString = true;
		}

		private bool isValid(long key, long value)
		{
			return key != LongPairRangeSet_LongPair.earliest.Key && value != LongPairRangeSet_LongPair.earliest.Value && key != LongPairRangeSet_LongPair.latest.Key && value != LongPairRangeSet_LongPair.latest.Value;
		}

		public virtual bool contains(long key, long value)
		{

			BitArray rangeBitSet = rangeBitSetMap.get(key);
			if (rangeBitSet != null)
			{
				return rangeBitSet.Get(getSafeEntry(value));
			}
			return false;
		}

		public virtual Range<T> rangeContaining(long key, long value)
		{
			BitArray rangeBitSet = rangeBitSetMap.get(key);
			if (rangeBitSet != null)
			{
				if (!rangeBitSet.Get(getSafeEntry(value)))
				{
					// if position is not part of any range then return null
					return null;
				}
				int lowerValue = rangeBitSet.previousClearBit(getSafeEntry(value)) + 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final T lower = consumer.apply(key, lowerValue);
				T lower = consumer.apply(key, lowerValue);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final T upper = consumer.apply(key, Math.max(rangeBitSet.nextClearBit(getSafeEntry(value)) - 1, lowerValue));
				T upper = consumer.apply(key, Math.Max(rangeBitSet.nextClearBit(getSafeEntry(value)) - 1, lowerValue));
				return Range.closed(lower, upper);
			}
			return null;
		}

		public virtual void removeAtMost(long key, long value)
		{
			this.remove(Range.atMost(new LongPairRangeSet_LongPair(key, value)));
		}

		public virtual bool Empty
		{
			get
			{
				if (rangeBitSetMap.Empty)
				{
					return true;
				}
				AtomicBoolean isEmpty = new AtomicBoolean(false);
				rangeBitSetMap.forEach((key, val) =>
				{
				if (!isEmpty.get())
				{
					return;
				}
				isEmpty.set(val.Empty);
				});
				return isEmpty.get();
			}
		}

		public virtual void clear()
		{
			rangeBitSetMap.clear();
			updatedAfterCachedForSize = true;
			updatedAfterCachedForToString = true;
		}

		public virtual Range<T> span()
		{
			KeyValuePair<long, BitArray> firstSet = rangeBitSetMap.firstEntry();
			KeyValuePair<long, BitArray> lastSet = rangeBitSetMap.lastEntry();
			int first = firstSet.Value.nextSetBit(0);
			int last = lastSet.Value.previousSetBit(lastSet.Value.size());
			return Range.openClosed(consumer.apply(firstSet.Key, first - 1), consumer.apply(lastSet.Key, last));
		}

		public virtual IList<Range<T>> asRanges()
		{
			IList<Range<T>> ranges = new List<Range<T>>();
			forEach((range) =>
			{
			ranges.Add(range);
			return true;
			});
			return ranges;
		}

		public virtual void forEach(LongPairRangeSet_RangeProcessor<T> action)
		{
			forEach(action, consumer);
		}

		public virtual void forEach<T1>(LongPairRangeSet_RangeProcessor<T> action, LongPairRangeSet_LongPairConsumer<T1> consumer) where T1 : T
		{
			AtomicBoolean completed = new AtomicBoolean(false);
			rangeBitSetMap.forEach((key, set) =>
			{
			if (completed.get())
			{
				return;
			}
			if (set.Empty)
			{
				return;
			}
			int first = set.nextSetBit(0);
			int last = set.previousSetBit(set.size());
			int currentClosedMark = first;
			while (currentClosedMark != -1 && currentClosedMark <= last)
			{
				int nextOpenMark = set.nextClearBit(currentClosedMark);
				Range<T> range = Range.openClosed(consumer.apply(key, currentClosedMark - 1), consumer.apply(key, nextOpenMark - 1));
				if (!action.process(range))
				{
					completed.set(true);
					break;
				}
				currentClosedMark = set.nextSetBit(nextOpenMark);
			}
			});
		}

		public virtual Range<T> firstRange()
		{
			if (rangeBitSetMap.Empty)
			{
				return null;
			}
			KeyValuePair<long, BitArray> firstSet = rangeBitSetMap.firstEntry();
			int lower = firstSet.Value.nextSetBit(0);
			int upper = Math.Max(lower, firstSet.Value.nextClearBit(lower) - 1);
			return Range.openClosed(consumer.apply(firstSet.Key, lower - 1), consumer.apply(firstSet.Key, upper));
		}

		public virtual Range<T> lastRange()
		{
			if (rangeBitSetMap.Empty)
			{
				return null;
			}
			KeyValuePair<long, BitArray> lastSet = rangeBitSetMap.lastEntry();
			int upper = lastSet.Value.previousSetBit(lastSet.Value.size());
			int lower = Math.Min(lastSet.Value.previousClearBit(upper), upper);
			return Range.openClosed(consumer.apply(lastSet.Key, lower), consumer.apply(lastSet.Key, upper));
		}

		public virtual int size()
		{
			if (updatedAfterCachedForSize)
			{
				AtomicInteger size = new AtomicInteger(0);
				forEach((range) =>
				{
				size.AndIncrement;
				return true;
				});
				cachedSize = size.get();
				updatedAfterCachedForSize = false;
			}
			return cachedSize;
		}

		public override string ToString()
		{
			if (updatedAfterCachedForToString)
			{
				StringBuilder toString = new StringBuilder();
				AtomicBoolean first = new AtomicBoolean(true);
				if (toString != null)
				{
					toString.Append("[");
				}
				forEach((range) =>
				{
				if (!first.get())
				{
					toString.Append(",");
				}
				toString.Append(range);
				first.set(false);
				return true;
				});
				toString.Append("]");
				cachedToString = toString.ToString();
				updatedAfterCachedForToString = false;
			}
			return cachedToString;
		}

		/// <summary>
		/// Adds the specified range to this {@code RangeSet} (optional operation). That is, for equal range sets a and b,
		/// the result of {@code a.add(range)} is that {@code a} will be the minimal range set for which both
		/// {@code a.enclosesAll(b)} and {@code a.encloses(range)}.
		/// 
		/// <para>Note that {@code range} will merge given {@code range} with any ranges in the range set that are
		/// <seealso cref="Range.isConnected(Range) connected"/> with it. Moreover, if {@code range} is empty/invalid, this is a
		/// no-op.
		/// </para>
		/// </summary>
		public virtual void add(Range<LongPairRangeSet_LongPair> range)
		{
			LongPairRangeSet_LongPair lowerEndpoint = range.hasLowerBound() ? range.lowerEndpoint() : LongPairRangeSet_LongPair.earliest;
			LongPairRangeSet_LongPair upperEndpoint = range.hasUpperBound() ? range.upperEndpoint() : LongPairRangeSet_LongPair.latest;

			long lowerValueOpen = (range.hasLowerBound() && range.lowerBoundType().Equals(BoundType.CLOSED)) ? getSafeEntry(lowerEndpoint) - 1 : getSafeEntry(lowerEndpoint);
			long upperValueClosed = (range.hasUpperBound() && range.upperBoundType().Equals(BoundType.CLOSED)) ? getSafeEntry(upperEndpoint) : getSafeEntry(upperEndpoint) + 1;

			// #addOpenClosed doesn't create bitSet for lower-key because it avoids setting up values for non-exist items
			// into the key-ledger. so, create bitSet and initialize so, it can't be ignored at #addOpenClosed
			rangeBitSetMap.computeIfAbsent(lowerEndpoint.Key, (key) => createNewBitSet()).set((int) lowerValueOpen + 1);
			this.addOpenClosed(lowerEndpoint.Key, lowerValueOpen, upperEndpoint.Key, upperValueClosed);
		}

		public virtual bool contains(LongPairRangeSet_LongPair position)
		{
			checkNotNull(position, "argument can't be null");
			return contains(position.Key, position.Value);
		}

		public virtual void remove(Range<LongPairRangeSet_LongPair> range)
		{
			LongPairRangeSet_LongPair lowerEndpoint = range.hasLowerBound() ? range.lowerEndpoint() : LongPairRangeSet_LongPair.earliest;
			LongPairRangeSet_LongPair upperEndpoint = range.hasUpperBound() ? range.upperEndpoint() : LongPairRangeSet_LongPair.latest;

			long lower = (range.hasLowerBound() && range.lowerBoundType().Equals(BoundType.CLOSED)) ? getSafeEntry(lowerEndpoint) : getSafeEntry(lowerEndpoint) + 1;
			long upper = (range.hasUpperBound() && range.upperBoundType().Equals(BoundType.CLOSED)) ? getSafeEntry(upperEndpoint) : getSafeEntry(upperEndpoint) - 1;

			// if lower-bound is not set then remove all the keys less than given upper-bound range
			if (lowerEndpoint.Equals(LongPairRangeSet_LongPair.earliest))
			{
				// remove all keys with
				rangeBitSetMap.forEach((key, set) =>
				{
				if (key < upperEndpoint.Key)
				{
					rangeBitSetMap.remove(key);
				}
				});
			}

			// if upper-bound is not set then remove all the keys greater than given lower-bound range
			if (upperEndpoint.Equals(LongPairRangeSet_LongPair.latest))
			{
				// remove all keys with
				rangeBitSetMap.forEach((key, set) =>
				{
				if (key > lowerEndpoint.Key)
				{
					rangeBitSetMap.remove(key);
				}
				});
			}

			// remove all the keys between two endpoint keys
			rangeBitSetMap.forEach((key, set) =>
			{
			if (lowerEndpoint.Key == upperEndpoint.Key)
			{
				set.clear((int) lower, (int) upper + 1);
			}
			else
			{
				if (key == lowerEndpoint.Key)
				{
					set.clear((int) lower, set.previousSetBit(set.size()));
				}
				else if (key == upperEndpoint.Key)
				{
					set.clear(0, (int) upper + 1);
				}
				else if (key > lowerEndpoint.Key && key < upperEndpoint.Key)
				{
					rangeBitSetMap.remove(key);
				}
			}
			if (set.Empty)
			{
				rangeBitSetMap.remove(key);
			}
			});

			updatedAfterCachedForSize = true;
			updatedAfterCachedForToString = true;
		}

		private int getSafeEntry(LongPairRangeSet_LongPair position)
		{
			return (int) Math.Max(position.Value, -1);
		}

		private int getSafeEntry(long value)
		{
			return (int) Math.Max(value, -1);
		}

		private BitArray createNewBitSet()
		{
			return this.threadSafe ? new ConcurrentBitSet(bitSetSize) : new BitArray(bitSetSize);
		}

	}

}