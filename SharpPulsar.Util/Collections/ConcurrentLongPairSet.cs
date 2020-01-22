using System;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	/// <summary>
	/// Concurrent hash set where values are composed of pairs of longs.
	/// 
	/// <para>Provides similar methods as a {@code ConcurrentHashSet<V>} but since it's an open hash set with linear probing,
	/// no node allocations are required to store the keys and values, and no boxing is required.
	/// 
	/// </para>
	/// <para>Values <b>MUST</b> be &gt;= 0.
	/// </para>
	/// </summary>
	public class ConcurrentLongPairSet : LongPairSet
	{

		private const long EmptyItem = -1L;
		private const long DeletedItem = -2L;

		private const float SetFillFactor = 0.66f;

		private const int DefaultExpectedItems = 256;
		private const int DefaultConcurrencyLevel = 16;

		private readonly Section[] sections;

		/// <summary>
		/// Represents a function that accepts an object of the {@code LongPair} type.
		/// </summary>
		public interface ConsumerLong
		{
			void accept(LongPair item);
		}

		/// <summary>
		/// Represents a function that accepts two long arguments.
		/// </summary>
		public interface LongPairConsumer
		{
			void accept(long v1, long v2);
		}

		public ConcurrentLongPairSet() : this(DefaultExpectedItems)
		{
		}

		public ConcurrentLongPairSet(int expectedItems) : this(expectedItems, DefaultConcurrencyLevel)
		{
		}

		public ConcurrentLongPairSet(int expectedItems, int concurrencyLevel)
		{
			checkArgument(expectedItems > 0);
			checkArgument(concurrencyLevel > 0);
			checkArgument(expectedItems >= concurrencyLevel);

			int numSections = concurrencyLevel;
			int perSectionExpectedItems = expectedItems / numSections;
			int perSectionCapacity = (int)(perSectionExpectedItems / SetFillFactor);
			this.sections = new Section[numSections];

			for (int i = 0; i < numSections; i++)
			{
				sections[i] = new Section(perSectionCapacity);
			}
		}

		public virtual long size()
		{
			long size = 0;
			foreach (Section s in sections)
			{
				size += s.size;
			}
			return size;
		}

		public virtual long capacity()
		{
			long capacity = 0;
			foreach (Section s in sections)
			{
				capacity += s.capacity;
			}
			return capacity;
		}

		public virtual bool Empty
		{
			get
			{
				foreach (Section s in sections)
				{
					if (s.size != 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		internal virtual long UsedBucketCount
		{
			get
			{
				long usedBucketCount = 0;
				foreach (Section s in sections)
				{
					usedBucketCount += s.usedBuckets;
				}
				return usedBucketCount;
			}
		}

		public virtual bool contains(long item1, long item2)
		{
			checkBiggerEqualZero(item1);
			long h = hash(item1, item2);
			return getSection(h).contains(item1, item2, (int) h);
		}

		public virtual bool add(long item1, long item2)
		{
			checkBiggerEqualZero(item1);
			long h = hash(item1, item2);
			return getSection(h).add(item1, item2, (int) h);
		}

		/// <summary>
		/// Remove an existing entry if found.
		/// </summary>
		/// <param name="item1"> </param>
		/// <returns> true if removed or false if item was not present </returns>
		public virtual bool remove(long item1, long item2)
		{
			checkBiggerEqualZero(item1);
			long h = hash(item1, item2);
			return getSection(h).remove(item1, item2, (int) h);
		}

		private Section getSection(long hash)
		{
			// Use 32 msb out of long to get the section
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sectionIdx = (int)(hash >>> 32) & (sections.length - 1);
			int sectionIdx = (int)((long)((ulong)hash >> 32)) & (sections.Length - 1);
			return sections[sectionIdx];
		}

		public virtual void clear()
		{
			foreach (Section s in sections)
			{
				s.clear();
			}
		}

		public virtual void forEach(LongPairConsumer processor)
		{
			foreach (Section s in sections)
			{
				s.forEach(processor);
			}
		}

		/// <summary>
		/// Removes all of the elements of this collection that satisfy the given predicate.
		/// </summary>
		/// <param name="filter">
		///            a predicate which returns {@code true} for elements to be removed
		/// </param>
		/// <returns> number of removed values </returns>
		public virtual int removeIf(LongPairSet_LongPairPredicate filter)
		{
			int removedValues = 0;
			foreach (Section s in sections)
			{
				removedValues += s.removeIf(filter);
			}
			return removedValues;
		}

		/// <returns> a new list of all keys (makes a copy) </returns>
		public virtual ISet<LongPair> items()
		{
			ISet<LongPair> items = new HashSet<LongPair>();
			forEach((item1, item2) => items.Add(new LongPair(item1, item2)));
			return items;
		}

		/// <returns> a new list of keys with max provided numberOfItems (makes a copy) </returns>
		public virtual ISet<LongPair> items(int numberOfItems)
		{
			return items(numberOfItems, (item1, item2) => new LongPair(item1, item2));
		}

		public virtual ISet<T> items<T>(int numberOfItems, LongPairSet_LongPairFunction<T> longPairConverter)
		{
			ISet<T> items = new HashSet<T>();
			foreach (Section s in sections)
			{
				s.forEach((item1, item2) =>
				{
				if (items.Count < numberOfItems)
				{
					items.Add(longPairConverter.apply(item1, item2));
				}
				});
				if (items.Count >= numberOfItems)
				{
					return items;
				}
			}
			return items;
		}

		// A section is a portion of the hash map that is covered by a single
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static final class Section extends java.util.concurrent.locks.StampedLock
		private sealed class Section : StampedLock
		{
			// Keys and values are stored interleaved in the table array
			internal volatile long[] table;

			internal volatile int capacity;
			internal volatile int size;
			internal int usedBuckets;
			internal int resizeThreshold;

			internal Section(int capacity)
			{
				this.capacity = alignToPowerOfTwo(capacity);
				this.table = new long[2 * this.capacity];
				this.size = 0;
				this.usedBuckets = 0;
				this.resizeThreshold = (int)(this.capacity * SetFillFactor);
				Arrays.fill(table, EmptyItem);
			}

			internal bool contains(long item1, long item2, int hash)
			{
				long stamp = tryOptimisticRead();
				bool acquiredLock = false;
				int bucket = signSafeMod(hash, capacity);

				try
				{
					while (true)
					{
						// First try optimistic locking
						long storedItem1 = table[bucket];
						long storedItem2 = table[bucket + 1];

						if (!acquiredLock && validate(stamp))
						{
							// The values we have read are consistent
							if (item1 == storedItem1 && item2 == storedItem2)
							{
								return true;
							}
							else if (storedItem1 == EmptyItem)
							{
								// Not found
								return false;
							}
						}
						else
						{
							// Fallback to acquiring read lock
							if (!acquiredLock)
							{
								stamp = readLock();
								acquiredLock = true;

								bucket = signSafeMod(hash, capacity);
								storedItem1 = table[bucket];
								storedItem2 = table[bucket + 1];
							}

							if (item1 == storedItem1 && item2 == storedItem2)
							{
								return true;
							}
							else if (storedItem1 == EmptyItem)
							{
								// Not found
								return false;
							}
						}

						bucket = (bucket + 2) & (table.Length - 1);
					}
				}
				finally
				{
					if (acquiredLock)
					{
						unlockRead(stamp);
					}
				}
			}

			internal bool add(long item1, long item2, long hash)
			{
				long stamp = writeLock();
				int bucket = signSafeMod(hash, capacity);

				// Remember where we find the first available spot
				int firstDeletedItem = -1;

				try
				{
					while (true)
					{
						long storedItem1 = table[bucket];
						long storedItem2 = table[bucket + 1];

						if (item1 == storedItem1 && item2 == storedItem2)
						{
							// Item was already in set
							return false;
						}
						else if (storedItem1 == EmptyItem)
						{
							// Found an empty bucket. This means the key is not in the set. If we've already seen a deleted
							// key, we should write at that position
							if (firstDeletedItem != -1)
							{
								bucket = firstDeletedItem;
							}
							else
							{
								++usedBuckets;
							}

							table[bucket] = item1;
							table[bucket + 1] = item2;
							++size;
							return true;
						}
						else if (storedItem1 == DeletedItem)
						{
							// The bucket contained a different deleted key
							if (firstDeletedItem == -1)
							{
								firstDeletedItem = bucket;
							}
						}

						bucket = (bucket + 2) & (table.Length - 1);
					}
				}
				finally
				{
					if (usedBuckets > resizeThreshold)
					{
						try
						{
							rehash();
						}
						finally
						{
							unlockWrite(stamp);
						}
					}
					else
					{
						unlockWrite(stamp);
					}
				}
			}

			internal bool remove(long item1, long item2, int hash)
			{
				long stamp = writeLock();
				int bucket = signSafeMod(hash, capacity);

				try
				{
					while (true)
					{
						long storedItem1 = table[bucket];
						long storedItem2 = table[bucket + 1];
						if (item1 == storedItem1 && item2 == storedItem2)
						{
							--size;

							cleanBucket(bucket);
							return true;

						}
						else if (storedItem1 == EmptyItem)
						{
							return false;
						}

						bucket = (bucket + 2) & (table.Length - 1);
					}
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			internal int removeIf(LongPairSet_LongPairPredicate filter)
			{
				Objects.requireNonNull(filter);
				int removedItems = 0;

				// Go through all the buckets for this section
				for (int bucket = 0; bucket < table.Length; bucket += 2)
				{
					long storedItem1 = table[bucket];
					long storedItem2 = table[bucket + 1];

					if (storedItem1 != DeletedItem && storedItem1 != EmptyItem)
					{
						if (filter.test(storedItem1, storedItem2))
						{
							long h = hash(storedItem1, storedItem2);
							if (remove(storedItem1, storedItem2, (int) h))
							{
								removedItems++;
							}
						}
					}
				}

				return removedItems;
			}

			internal void cleanBucket(int bucket)
			{
				int nextInArray = (bucket + 2) & (table.Length - 1);
				if (table[nextInArray] == EmptyItem)
				{
					table[bucket] = EmptyItem;
					table[bucket + 1] = EmptyItem;
					--usedBuckets;
				}
				else
				{
					table[bucket] = DeletedItem;
					table[bucket + 1] = DeletedItem;
				}
			}

			internal void clear()
			{
				long stamp = writeLock();

				try
				{
					Arrays.fill(table, EmptyItem);
					this.size = 0;
					this.usedBuckets = 0;
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			public void forEach(LongPairConsumer processor)
			{
				long stamp = tryOptimisticRead();

				long[] table = this.table;
				bool acquiredReadLock = false;

				try
				{

					// Validate no rehashing
					if (!validate(stamp))
					{
						// Fallback to read lock
						stamp = readLock();
						acquiredReadLock = true;
						table = this.table;
					}

					// Go through all the buckets for this section
					for (int bucket = 0; bucket < table.Length; bucket += 2)
					{
						long storedItem1 = table[bucket];
						long storedItem2 = table[bucket + 1];

						if (!acquiredReadLock && !validate(stamp))
						{
							// Fallback to acquiring read lock
							stamp = readLock();
							acquiredReadLock = true;

							storedItem1 = table[bucket];
							storedItem2 = table[bucket + 1];
						}

						if (storedItem1 != DeletedItem && storedItem1 != EmptyItem)
						{
							processor.accept(storedItem1, storedItem2);
						}
					}
				}
				finally
				{
					if (acquiredReadLock)
					{
						unlockRead(stamp);
					}
				}
			}

			internal void rehash()
			{
				// Expand the hashmap
				int newCapacity = capacity * 2;
				long[] newTable = new long[2 * newCapacity];
				Arrays.fill(newTable, EmptyItem);

				// Re-hash table
				for (int i = 0; i < table.Length; i += 2)
				{
					long storedItem1 = table[i];
					long storedItem2 = table[i + 1];
					if (storedItem1 != EmptyItem && storedItem1 != DeletedItem)
					{
						insertKeyValueNoLock(newTable, newCapacity, storedItem1, storedItem2);
					}
				}

				table = newTable;
				usedBuckets = size;
				// Capacity needs to be updated after the values, so that we won't see
				// a capacity value bigger than the actual array size
				capacity = newCapacity;
				resizeThreshold = (int)(capacity * SetFillFactor);
			}

			internal static void insertKeyValueNoLock(long[] table, int capacity, long item1, long item2)
			{
				int bucket = signSafeMod(hash(item1, item2), capacity);

				while (true)
				{
					long storedKey = table[bucket];

					if (storedKey == EmptyItem)
					{
						// The bucket is empty, so we can use it
						table[bucket] = item1;
						table[bucket + 1] = item2;
						return;
					}

					bucket = (bucket + 2) & (table.Length - 1);
				}
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

		internal static int signSafeMod(long n, int max)
		{
			return (int)(n & (max - 1)) << 1;
		}

		private static int alignToPowerOfTwo(int n)
		{
			return (int) Math.Pow(2, 32 - Integer.numberOfLeadingZeros(n - 1));
		}

		private static void checkBiggerEqualZero(long n)
		{
			if (n < 0L)
			{
				throw new System.ArgumentException("Keys and values must be >= 0");
			}
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
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean first = new java.util.concurrent.atomic.AtomicBoolean(true);
			AtomicBoolean first = new AtomicBoolean(true);
			forEach((item1, item2) =>
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
			sb.Append('}');
			return sb.ToString();
		}

	}

}