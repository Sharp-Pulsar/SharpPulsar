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
namespace org.apache.pulsar.common.util.collections
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Lists = com.google.common.collect.Lists;

	/// <summary>
	/// Concurrent hash set.
	/// 
	/// <para>Provides similar methods as a {@code ConcurrentMap<K,V>} but since it's an open hash map with linear probing,
	/// no node allocations are required to store the values.
	/// 
	/// </para>
	/// </summary>
	/// @param <V> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class ConcurrentOpenHashSet<V>
	public class ConcurrentOpenHashSet<V>
	{

		private const object EmptyValue = null;
		private static readonly object DeletedValue = new object();

		private const float MapFillFactor = 0.66f;

		private const int DefaultExpectedItems = 256;
		private const int DefaultConcurrencyLevel = 16;

		private readonly Section<V>[] sections;

		public ConcurrentOpenHashSet() : this(DefaultExpectedItems)
		{
		}

		public ConcurrentOpenHashSet(int expectedItems) : this(expectedItems, DefaultConcurrencyLevel)
		{
		}

		public ConcurrentOpenHashSet(int expectedItems, int concurrencyLevel)
		{
			checkArgument(expectedItems > 0);
			checkArgument(concurrencyLevel > 0);
			checkArgument(expectedItems >= concurrencyLevel);

			int numSections = concurrencyLevel;
			int perSectionExpectedItems = expectedItems / numSections;
			int perSectionCapacity = (int)(perSectionExpectedItems / MapFillFactor);
			this.sections = (Section<V>[]) new Section[numSections];

			for (int i = 0; i < numSections; i++)
			{
				sections[i] = new Section<V>(perSectionCapacity);
			}
		}

		public virtual long size()
		{
			long size = 0;
			foreach (Section<V> s in sections)
			{
				size += s.size;
			}
			return size;
		}

		public virtual long capacity()
		{
			long capacity = 0;
			foreach (Section<V> s in sections)
			{
				capacity += s.capacity;
			}
			return capacity;
		}

		public virtual bool Empty
		{
			get
			{
				foreach (Section<V> s in sections)
				{
					if (s.size != 0)
					{
						return false;
					}
				}
    
				return true;
			}
		}

		public virtual bool contains(V value)
		{
			checkNotNull(value);
			long h = hash(value);
			return getSection(h).contains(value, (int) h);
		}

		public virtual bool add(V value)
		{
			checkNotNull(value);
			long h = hash(value);
			return getSection(h).add(value, (int) h);
		}

		public virtual bool remove(V value)
		{
			checkNotNull(value);
			long h = hash(value);
			return getSection(h).remove(value, (int) h);
		}

		private Section<V> getSection(long hash)
		{
			// Use 32 msb out of long to get the section
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sectionIdx = (int)(hash >>> 32) & (sections.length - 1);
			int sectionIdx = (int)((long)((ulong)hash >> 32)) & (sections.Length - 1);
			return sections[sectionIdx];
		}

		public virtual void clear()
		{
			foreach (Section<V> s in sections)
			{
				s.clear();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEach(java.util.function.Consumer<? super V> processor)
		public virtual void forEach<T1>(System.Action<T1> processor)
		{
			foreach (Section<V> s in sections)
			{
				s.forEach(processor);
			}
		}

		public virtual int removeIf(System.Predicate<V> filter)
		{
			checkNotNull(filter);

			int removedCount = 0;
			foreach (Section<V> s in sections)
			{
				removedCount += s.removeIf(filter);
			}

			return removedCount;
		}

		/// <returns> a new list of all values (makes a copy) </returns>
		public virtual IList<V> values()
		{
			IList<V> values = Lists.newArrayList();
			forEach(value => values.Add(value));
			return values;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean first = new java.util.concurrent.atomic.AtomicBoolean(true);
			AtomicBoolean first = new AtomicBoolean(true);
			forEach(value =>
			{
			if (!first.getAndSet(false))
			{
				sb.Append(", ");
			}
			sb.Append(value.ToString());
			});
			sb.Append('}');
			return sb.ToString();
		}

		// A section is a portion of the hash map that is covered by a single
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static final class Section<V> extends java.util.concurrent.locks.StampedLock
		private sealed class Section<V> : StampedLock
		{
			internal volatile V[] values;

			internal volatile int capacity;
			internal volatile int size;
			internal int usedBuckets;
			internal int resizeThreshold;

			internal Section(int capacity)
			{
				this.capacity = alignToPowerOfTwo(capacity);
				this.values = (V[]) new object[this.capacity];
				this.size = 0;
				this.usedBuckets = 0;
				this.resizeThreshold = (int)(this.capacity * MapFillFactor);
			}

			internal bool contains(V value, int keyHash)
			{
				int bucket = keyHash;

				long stamp = tryOptimisticRead();
				bool acquiredLock = false;

				try
				{
					while (true)
					{
						int capacity = this.capacity;
						bucket = signSafeMod(bucket, capacity);

						// First try optimistic locking
						V storedValue = values[bucket];

						if (!acquiredLock && validate(stamp))
						{
							// The values we have read are consistent
							if (value.Equals(storedValue))
							{
								return true;
							}
							else if (storedValue == EmptyValue)
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

								storedValue = values[bucket];
							}

							if (capacity != this.capacity)
							{
								// There has been a rehashing. We need to restart the search
								bucket = keyHash;
								continue;
							}

							if (value.Equals(storedValue))
							{
								return true;
							}
							else if (storedValue == EmptyValue)
							{
								// Not found
								return false;
							}
						}

						++bucket;
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

			internal bool add(V value, int keyHash)
			{
				int bucket = keyHash;

				long stamp = writeLock();
				int capacity = this.capacity;

				// Remember where we find the first available spot
				int firstDeletedValue = -1;

				try
				{
					while (true)
					{
						bucket = signSafeMod(bucket, capacity);

						V storedValue = values[bucket];

						if (value.Equals(storedValue))
						{
							return false;
						}
						else if (storedValue == EmptyValue)
						{
							// Found an empty bucket. This means the value is not in the set. If we've already seen a
							// deleted value, we should write at that position
							if (firstDeletedValue != -1)
							{
								bucket = firstDeletedValue;
							}
							else
							{
								++usedBuckets;
							}

							values[bucket] = value;
							++size;
							return true;
						}
						else if (storedValue == DeletedValue)
						{
							// The bucket contained a different deleted key
							if (firstDeletedValue == -1)
							{
								firstDeletedValue = bucket;
							}
						}

						++bucket;
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

			internal bool remove(V value, int keyHash)
			{
				int bucket = keyHash;
				long stamp = writeLock();

				try
				{
					while (true)
					{
						int capacity = this.capacity;
						bucket = signSafeMod(bucket, capacity);

						V storedValue = values[bucket];
						if (value.Equals(storedValue))
						{
							--size;
							cleanBucket(bucket);
							return true;
						}
						else if (storedValue == EmptyValue)
						{
							// Value wasn't found
							return false;
						}

						++bucket;
					}

				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			internal void clear()
			{
				long stamp = writeLock();

				try
				{
					Arrays.fill(values, EmptyValue);
					this.size = 0;
					this.usedBuckets = 0;
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			internal int removeIf(System.Predicate<V> filter)
			{
				long stamp = writeLock();

				int removedCount = 0;
				try
				{
					// Go through all the buckets for this section
					for (int bucket = capacity - 1; bucket >= 0; bucket--)
					{
						V storedValue = values[bucket];

						if (storedValue != DeletedValue && storedValue != EmptyValue)
						{
							if (filter(storedValue))
							{
								// Removing item
								--size;
								++removedCount;
								cleanBucket(bucket);
							}
						}
					}

					return removedCount;
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			internal void cleanBucket(int bucket)
			{
				int nextInArray = signSafeMod(bucket + 1, capacity);
				if (values[nextInArray] == EmptyValue)
				{
					values[bucket] = (V) EmptyValue;
					--usedBuckets;
				}
				else
				{
					values[bucket] = (V) DeletedValue;
				}
			}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEach(java.util.function.Consumer<? super V> processor)
			public void forEach<T1>(System.Action<T1> processor)
			{
				long stamp = tryOptimisticRead();

				int capacity = this.capacity;
				V[] values = this.values;

				bool acquiredReadLock = false;

				try
				{

					// Validate no rehashing
					if (!validate(stamp))
					{
						// Fallback to read lock
						stamp = readLock();
						acquiredReadLock = true;

						capacity = this.capacity;
						values = this.values;
					}

					// Go through all the buckets for this section
					for (int bucket = 0; bucket < capacity; bucket++)
					{
						V storedValue = values[bucket];

						if (!acquiredReadLock && !validate(stamp))
						{
							// Fallback to acquiring read lock
							stamp = readLock();
							acquiredReadLock = true;

							storedValue = values[bucket];
						}

						if (storedValue != DeletedValue && storedValue != EmptyValue)
						{
							processor(storedValue);
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
				V[] newValues = (V[]) new object[newCapacity];

				// Re-hash table
				for (int i = 0; i < values.Length; i++)
				{
					V storedValue = values[i];
					if (storedValue != EmptyValue && storedValue != DeletedValue)
					{
						insertValueNoLock(newValues, storedValue);
					}
				}

				values = newValues;
				capacity = newCapacity;
				usedBuckets = size;
				resizeThreshold = (int)(capacity * MapFillFactor);
			}

			internal static void insertValueNoLock<V>(V[] values, V value)
			{
				int bucket = (int) hash(value);

				while (true)
				{
					bucket = signSafeMod(bucket, values.Length);

					V storedValue = values[bucket];

					if (storedValue == EmptyValue)
					{
						// The bucket is empty, so we can use it
						values[bucket] = value;
						return;
					}

					++bucket;
				}
			}
		}

		private const long HashMixer = unchecked((long)0xc6a4a7935bd1e995L);
		private const int R = 47;

		internal static long hash<K>(K key)
		{
			long hash = key.GetHashCode() * HashMixer;
			hash ^= (long)((ulong)hash >> R);
			hash *= HashMixer;
			return hash;
		}

		internal static int signSafeMod(long n, int max)
		{
			return (int) n & (max - 1);
		}

		private static int alignToPowerOfTwo(int n)
		{
			return (int) Math.Pow(2, 32 - Integer.numberOfLeadingZeros(n - 1));
		}
	}

}