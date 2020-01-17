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
namespace org.apache.pulsar.common.util.collections
{
	using ComparisonChain = com.google.common.collect.ComparisonChain;
	using Lists = com.google.common.collect.Lists;
	using Range = com.google.common.collect.Range;
	using RangeSet = com.google.common.collect.RangeSet;
	using TreeRangeSet = com.google.common.collect.TreeRangeSet;


	/// <summary>
	/// A set comprising zero or more ranges type of key-value pair.
	/// </summary>
	public interface LongPairRangeSet<T> where T : IComparable<T>
	{

		/// <summary>
		/// Adds the specified range (range that contains all values strictly greater than {@code
		/// lower} and less than or equal to {@code upper}.) to this {@code RangeSet} (optional operation). That is, for equal
		/// range sets a and b, the result of {@code a.add(range)} is that {@code a} will be the minimal range set for which
		/// both {@code a.enclosesAll(b)} and {@code a.encloses(range)}.
		/// 
		/// <pre>
		/// 
		/// &#64;param lowerKey :  value for key of lowerEndpoint of Range
		/// &#64;param lowerValue: value for value of lowerEndpoint of Range
		/// &#64;param upperKey  : value for key of upperEndpoint of Range
		/// &#64;param upperValue: value for value of upperEndpoint of Range
		/// </pre>
		/// </summary>
		void addOpenClosed(long lowerKey, long lowerValue, long upperKey, long upperValue);

		/// <summary>
		/// Determines whether any of this range set's member ranges contains {@code value}. </summary>
		bool contains(long key, long value);

		/// <summary>
		/// Returns the unique range from this range set that <seealso cref="Range.contains contains"/> {@code value}, or
		/// {@code null} if this range set does not contain {@code value}.
		/// </summary>
		Range<T> rangeContaining(long key, long value);

		/// <summary>
		/// Remove range that contains all values less than or equal to given key-value.
		/// </summary>
		/// <param name="key"> </param>
		/// <param name="value"> </param>
		void removeAtMost(long key, long value);

		bool Empty {get;}

		void clear();

		/// <summary>
		/// Returns the minimal range which <seealso cref="Range.encloses(Range) encloses"/> all ranges in this range set.
		/// 
		/// @return
		/// </summary>
		Range<T> span();

		/// <summary>
		/// Returns a view of the <seealso cref="Range.isConnected disconnected"/> ranges that make up this range set.
		/// 
		/// @return
		/// </summary>
		ICollection<Range<T>> asRanges();

		/// <summary>
		/// Performs the given action for each entry in this map until all entries have been processed
		/// or action returns "false". Unless otherwise specified by the implementing class,
		/// actions are performed in the order of entry set iteration (if an iteration order is specified.)
		/// </summary>
		/// <param name="action"> </param>
		void forEach(LongPairRangeSet_RangeProcessor<T> action);

		/// <summary>
		/// Performs the given action for each entry in this map until all entries have been processed
		/// or action returns "false". Unless otherwise specified by the implementing class,
		/// actions are performed in the order of entry set iteration (if an iteration order is specified.)
		/// </summary>
		/// <param name="action"> </param>
		/// <param name="consumer"> </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void forEach(LongPairRangeSet_RangeProcessor<T> action, LongPairRangeSet_LongPairConsumer<? extends T> consumer);
		void forEach<T1>(LongPairRangeSet_RangeProcessor<T> action, LongPairRangeSet_LongPairConsumer<T1> consumer);

		/// <summary>
		/// Returns total number of ranges into the set.
		/// 
		/// @return
		/// </summary>
		int size();

		/// <summary>
		/// It returns very first smallest range in the rangeSet.
		/// </summary>
		/// <returns> first smallest range into the set </returns>
		Range<T> firstRange();

		/// <summary>
		/// It returns very last biggest range in the rangeSet.
		/// </summary>
		/// <returns> last biggest range into the set </returns>
		Range<T> lastRange();

		/// <summary>
		/// Represents a function that accepts two long arguments and produces a result.
		/// </summary>
		/// @param <T> the type of the result. </param>

		/// <summary>
		/// The interface exposing a method for processing of ranges. </summary>
		/// @param <T> - The incoming type of data in the range object. </param>

		/// <summary>
		/// This class is a simple key-value data structure.
		/// </summary>

		/// <summary>
		/// Generic implementation of a default range set.
		/// </summary>
		/// @param <T> the type of values in ranges. </param>
	}

	public interface LongPairRangeSet_LongPairConsumer<T>
	{
		T apply(long key, long value);
	}

	public interface LongPairRangeSet_RangeProcessor<T> where T : IComparable<T>
	{
		/// 
		/// <param name="range"> </param>
		/// <returns> false if there is no further processing required </returns>
		bool process(Range<T> range);
	}

	public class LongPairRangeSet_LongPair : IComparable<LongPairRangeSet_LongPair>
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:ConstantName") public static final LongPairRangeSet_LongPair earliest = new LongPairRangeSet_LongPair(-1, -1);
		public static readonly LongPairRangeSet_LongPair earliest = new LongPairRangeSet_LongPair(-1, -1);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:ConstantName") public static final LongPairRangeSet_LongPair latest = new LongPairRangeSet_LongPair(Integer.MAX_VALUE, Integer.MAX_VALUE);
		public static readonly LongPairRangeSet_LongPair latest = new LongPairRangeSet_LongPair(int.MaxValue, int.MaxValue);

		internal long key;
		internal long value;

		public LongPairRangeSet_LongPair(long key, long value)
		{
			this.key = key;
			this.value = value;
		}

		public virtual long Key
		{
			get
			{
				return this.key;
			}
		}

		public virtual long Value
		{
			get
			{
				return this.value;
			}
		}

		public virtual int CompareTo(LongPairRangeSet_LongPair o)
		{
			return ComparisonChain.start().compare(key, o.Key).compare(value, o.Value).result();
		}

		public override string ToString()
		{
			return string.Format("{0:D}:{1:D}", key, value);
		}
	}

	public class LongPairRangeSet_DefaultRangeSet<T> : LongPairRangeSet<T> where T : IComparable<T>
	{

		internal RangeSet<T> set = TreeRangeSet.create();

		internal readonly LongPairRangeSet_LongPairConsumer<T> consumer;

		public LongPairRangeSet_DefaultRangeSet(LongPairRangeSet_LongPairConsumer<T> consumer)
		{
			this.consumer = consumer;
		}

		public virtual void clear()
		{
			set.clear();
		}

		public virtual void addOpenClosed(long key1, long value1, long key2, long value2)
		{
			set.add(Range.openClosed(consumer.apply(key1, value1), consumer.apply(key2, value2)));
		}

		public virtual bool contains(T position)
		{
			return set.contains(position);
		}

		public virtual Range<T> rangeContaining(T position)
		{
			return set.rangeContaining(position);
		}

		public virtual Range<T> rangeContaining(long key, long value)
		{
			return this.rangeContaining(consumer.apply(key, value));
		}

		public virtual void remove(Range<T> range)
		{
			set.remove(range);
		}

		public virtual void removeAtMost(long key, long value)
		{
			set.remove(Range.atMost(consumer.apply(key, value)));
		}

		public virtual bool Empty
		{
			get
			{
				return set.Empty;
			}
		}

		public virtual Range<T> span()
		{
			return set.span();
		}

		public virtual ISet<Range<T>> asRanges()
		{
			return set.asRanges();
		}

		public virtual void forEach(LongPairRangeSet_RangeProcessor<T> action)
		{
			forEach(action, consumer);
		}

		public virtual void forEach<T1>(LongPairRangeSet_RangeProcessor<T> action, LongPairRangeSet_LongPairConsumer<T1> consumer) where T1 : T
		{
			foreach (Range<T> range in asRanges())
			{
				if (!action.process(range))
				{
					break;
				}
			}
		}

		public virtual bool contains(long key, long value)
		{
			return this.contains(consumer.apply(key, value));
		}

		public virtual Range<T> firstRange()
		{
			return set.asRanges().GetEnumerator().next();
		}

		public virtual Range<T> lastRange()
		{
			if (set.asRanges().Empty)
			{
				return null;
			}
			IList<Range<T>> list = Lists.newArrayList(set.asRanges().GetEnumerator());
			return list[list.Count - 1];
		}

		public virtual int size()
		{
			return set.asRanges().size();
		}

		public override string ToString()
		{
			return set.ToString();
		}
	}

}