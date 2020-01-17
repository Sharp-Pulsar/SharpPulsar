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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	/// <summary>
	/// An unbounded priority queue based on a min heap where values are composed of pairs of longs.
	/// 
	/// <para>When the capacity is reached, data will be moved to a bigger array.
	/// 
	/// <b>It also act as a set and doesn't store duplicate values if <seealso cref="allowedDuplicate"/> flag is passed false</b>
	/// 
	/// </para>
	/// <para>(long,long)
	/// </para>
	/// </summary>
	public class GrowablePriorityLongPairQueue
	{

		private long[] data;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int capacity_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile int size_Conflict = 0;
		private const long EmptyItem = -1L;

		public GrowablePriorityLongPairQueue() : this(64)
		{
		}

		public GrowablePriorityLongPairQueue(int initialCapacity)
		{
			checkArgument(initialCapacity > 0);
			this.capacity_Conflict = io.netty.util.@internal.MathUtil.findNextPositivePowerOfTwo(initialCapacity);
			data = new long[2 * capacity_Conflict];
			Arrays.fill(data, 0, data.Length, EmptyItem);
		}

		/// <summary>
		/// Predicate to checks for a key-value pair where both of them have long types.
		/// </summary>
		public interface LongPairPredicate
		{
			bool test(long v1, long v2);
		}

		/// <summary>
		/// Represents a function that accepts two long arguments.
		/// </summary>
		public interface LongPairConsumer
		{
			void accept(long v1, long v2);
		}

		public virtual void add(long item1, long item2)
		{
			lock (this)
			{
        
				if (this.size_Conflict >= this.capacity_Conflict)
				{
					expandArray();
				}
        
				int lastIndex = this.size_Conflict << 1;
				data[lastIndex] = item1;
				data[lastIndex + 1] = item2;
        
				int loc = lastIndex;
        
				// Swap with parent until parent not larger
				while (loc > 0 && compare(loc, parent(loc)) < 0)
				{
					swap(loc, parent(loc));
					loc = parent(loc);
				}
        
				this.size_Conflict++;
        
			}
		}

		public virtual void forEach(LongPairConsumer processor)
		{
			lock (this)
			{
				int index = 0;
				for (int i = 0; i < this.size_Conflict; i++)
				{
					processor.accept(data[index], data[index + 1]);
					index = index + 2;
				}
			}
		}

		/// <returns> a new list of all keys (makes a copy) </returns>
		public virtual ISet<LongPair> items()
		{
			ISet<LongPair> items = new HashSet<LongPair>(this.size_Conflict);
			forEach((item1, item2) => items.Add(new LongPair(item1, item2)));
			return items;
		}

		/// <returns> a new list of keys with max provided numberOfItems (makes a copy) </returns>
		public virtual ISet<LongPair> items(int numberOfItems)
		{
			ISet<LongPair> items = new HashSet<LongPair>(this.size_Conflict);
			forEach((item1, item2) =>
			{
			if (items.Count < numberOfItems)
			{
				items.Add(new LongPair(item1, item2));
			}
			});

			return items;
		}

		/// <summary>
		/// Removes all of the elements of this collection that satisfy the given predicate.
		/// </summary>
		/// <param name="filter">
		///            a predicate which returns {@code true} for elements to be removed
		/// </param>
		/// <returns> number of removed values </returns>
		public virtual int removeIf(LongPairPredicate filter)
		{
			lock (this)
			{
				int removedValues = 0;
				int index = 0;
				long[] deletedItems = new long[size_Conflict * 2];
				int deleteItemsIndex = 0;
				// collect eligible items for deletion
				for (int i = 0; i < this.size_Conflict; i++)
				{
					if (filter.test(data[index], data[index + 1]))
					{
						deletedItems[deleteItemsIndex++] = data[index];
						deletedItems[deleteItemsIndex++] = data[index + 1];
						removedValues++;
					}
					index = index + 2;
				}
        
				// delete collected items
				deleteItemsIndex = 0;
				for (int deleteItem = 0; deleteItem < removedValues; deleteItem++)
				{
					// delete item from the heap
					index = 0;
					for (int i = 0; i < this.size_Conflict; i++)
					{
						if (data[index] == deletedItems[deleteItemsIndex] && data[index + 1] == deletedItems[deleteItemsIndex + 1])
						{
							removeAtWithoutLock(index);
						}
						index = index + 2;
					}
					deleteItemsIndex = deleteItemsIndex + 2;
				}
				return removedValues;
			}
		}

		/// <summary>
		/// It removes all occurrence of given pair from the queue.
		/// </summary>
		/// <param name="item1"> </param>
		/// <param name="item2">
		/// @return </param>
		public virtual bool remove(long item1, long item2)
		{
			lock (this)
			{
				bool removed = false;
        
				int index = 0;
				for (int i = 0; i < this.size_Conflict; i++)
				{
					if (data[index] == item1 && data[index + 1] == item2)
					{
						removeAtWithoutLock(index);
						removed = true;
					}
					index = index + 2;
				}
        
				return removed;
			}
		}

		/// <summary>
		/// Removes min element from the heap.
		/// 
		/// @return
		/// </summary>
		public virtual LongPair remove()
		{
			return removeAt(0);
		}

		private LongPair removeAt(int index)
		{
			lock (this)
			{
				return removeAtWithoutLock(index);
			}
		}

		/// <summary>
		/// it is not a thread-safe method and it should be called before acquiring a lock by a caller.
		/// </summary>
		/// <param name="index">
		/// @return </param>
		private LongPair removeAtWithoutLock(int index)
		{
			if (this.size_Conflict > 0)
			{
				LongPair item = new LongPair(data[index], data[index + 1]);
				data[index] = EmptyItem;
				data[index + 1] = EmptyItem;
				--this.size_Conflict;
				int lastIndex = this.size_Conflict << 1;
				swap(index, lastIndex);
				minHeapify(index, lastIndex - 2);
				return item;
			}
			else
			{
				return null;
			}
		}

		public virtual LongPair peek()
		{
			lock (this)
			{
				if (this.size_Conflict > 0)
				{
					return new LongPair(data[0], data[1]);
				}
				else
				{
					return null;
				}
			}
		}

		public virtual bool Empty
		{
			get
			{
				return this.size_Conflict == 0;
			}
		}

		public virtual int capacity()
		{
			return this.capacity_Conflict;
		}

		public virtual void clear()
		{
			lock (this)
			{
				int index = 0;
				for (int i = 0; i < this.size_Conflict; i++)
				{
					data[index] = -1;
					data[index + 1] = -1;
					index = index + 2;
				}
        
				this.size_Conflict = 0;
        
			}
		}

		public virtual int size()
		{
			return this.size_Conflict;
		}

		public virtual bool exists(long item1, long item2)
		{
			lock (this)
			{
        
				int index = 0;
				for (int i = 0; i < this.size_Conflict; i++)
				{
					if (data[index] == item1 && data[index + 1] == item2)
					{
						return true;
					}
					index = index + 2;
				}
				return false;
			}
		}

		private int compare(int index1, int index2)
		{
			if (data[index1] != data[index2])
			{
				return Long.compare(data[index1], data[index2]);
			}
			else
			{
				return Long.compare(data[index1 + 1], data[index2 + 1]);
			}
		}

		private void expandArray()
		{
			this.capacity_Conflict = capacity_Conflict * 2;
			long[] newData = new long[2 * this.capacity_Conflict];

			int index = 0;
			for (int i = 0; i < this.size_Conflict; i++)
			{
				newData[index] = data[index];
				newData[index + 1] = data[index + 1];
				index = index + 2;
			}
			Arrays.fill(newData, index, newData.Length, EmptyItem);
			data = newData;
		}

		private void swap(int i, int j)
		{
			long t = data[i];
			data[i] = data[j];
			data[j] = t;
			t = data[i + 1];
			data[i + 1] = data[j + 1];
			data[j + 1] = t;
		}

		private static int leftChild(int i)
		{
			return (i << 1) + 2;
		}

		private static int rightChild(int i)
		{
			return (i << 1) + 4;
		}

		private static int parent(int i)
		{
			return ((i - 2) >> 1) & ~1;
		}

		private void minHeapify(int index, int lastIndex)
		{
			int left = leftChild(index);
			int right = rightChild(index);
			int smallest;

			if (left <= lastIndex && compare(left, index) < 0)
			{
				smallest = left;
			}
			else
			{
				smallest = index;
			}
			if (right <= lastIndex && compare(right, smallest) < 0)
			{
				smallest = right;
			}
			if (smallest != index)
			{
				swap(index, smallest);
				minHeapify(smallest, lastIndex);
			}
		}

		private const long HashMixer = unchecked((long)0xc6a4a7935bd1e995L);
		private const int R = 47;

		internal static long hash(long key1, long key2)
		{
			long hash = key1 * HashMixer;
			hash ^= (long)((ulong)hash >> R);
			hash *= HashMixer;
			hash += 31 + (key2 * HashMixer);
			hash ^= (long)((ulong)hash >> R);
			hash *= HashMixer;
			return hash;
		}

		/// <summary>
		/// Class representing two long values.
		/// </summary>
		public class LongPair : IComparable<LongPair>
		{
			public readonly long first;
			public readonly long second;

			public LongPair(long first, long second)
			{
				this.first = first;
				this.second = second;
			}

			public override bool Equals(object obj)
			{
				if (obj is LongPair)
				{
					LongPair other = (LongPair) obj;
					return first == other.first && second == other.second;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (int) hash(first, second);
			}

			public virtual int CompareTo(LongPair o)
			{
				if (first != o.first)
				{
					return Long.compare(first, o.first);
				}
				else
				{
					return Long.compare(second, o.second);
				}
			}

			public override string ToString()
			{
				return "LongPair [first=" + first + ", second=" + second + "]";
			}

		}

	}

}