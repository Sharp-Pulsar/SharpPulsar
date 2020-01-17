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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Lists = com.google.common.collect.Lists;

	/// <summary>
	/// Map from long to an Object.
	/// 
	/// <para>Provides similar methods as a {@code ConcurrentMap<long,Object>} with 2 differences:
	/// <ol>
	/// <li>No boxing/unboxing from long -> Long
	/// <li>Open hash map with linear probing, no node allocations to store the values
	/// </ol>
	/// 
	/// </para>
	/// </summary>
	/// @param <V> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class ConcurrentLongHashMap<V>
	public class ConcurrentLongHashMap<V>
	{

		private const object EmptyValue = null;
		private static readonly object DeletedValue = new object();

		private const float MapFillFactor = 0.66f;

		private const int DefaultExpectedItems = 256;
		private const int DefaultConcurrencyLevel = 16;

		private readonly Section<V>[] sections;

		public ConcurrentLongHashMap() : this(DefaultExpectedItems)
		{
		}

		public ConcurrentLongHashMap(int expectedItems) : this(expectedItems, DefaultConcurrencyLevel)
		{
		}

		public ConcurrentLongHashMap(int expectedItems, int concurrencyLevel)
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

		internal virtual long UsedBucketCount
		{
			get
			{
				long usedBucketCount = 0;
				foreach (Section<V> s in sections)
				{
					usedBucketCount += s.usedBuckets;
				}
				return usedBucketCount;
			}
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

		public virtual V get(long key)
		{
			long h = hash(key);
			return getSection(h).get(key, (int) h);
		}

		public virtual bool containsKey(long key)
		{
			return get(key) != null;
		}

		public virtual V put(long key, V value)
		{
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).put(key, value, (int) h, false, null);
		}

		public virtual V putIfAbsent(long key, V value)
		{
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).put(key, value, (int) h, true, null);
		}

		public virtual V computeIfAbsent(long key, System.Func<long, V> provider)
		{
			checkNotNull(provider);
			long h = hash(key);
			return getSection(h).put(key, default(V), (int) h, true, provider);
		}

		public virtual V remove(long key)
		{
			long h = hash(key);
			return getSection(h).remove(key, null, (int) h);
		}

		public virtual bool remove(long key, object value)
		{
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).remove(key, value, (int) h) != null;
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

		public virtual void forEach(EntryProcessor<V> processor)
		{
			foreach (Section<V> s in sections)
			{
				s.forEach(processor);
			}
		}

		/// <returns> a new list of all keys (makes a copy) </returns>
		public virtual IList<long> keys()
		{
			IList<long> keys = Lists.newArrayListWithExpectedSize((int) size());
			forEach((key, value) => keys.Add(key));
			return keys;
		}

		public virtual IList<V> values()
		{
			IList<V> values = Lists.newArrayListWithExpectedSize((int) size());
			forEach((key, value) => values.Add(value));
			return values;
		}

		/// <summary>
		/// Processor for one key-value entry, where the key is {@code long}.
		/// </summary>
		/// @param <V> type of the value. </param>
		public interface EntryProcessor<V>
		{
			void accept(long key, V value);
		}

		// A section is a portion of the hash map that is covered by a single
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static final class Section<V> extends java.util.concurrent.locks.StampedLock
		private sealed class Section<V> : StampedLock
		{
			internal volatile long[] keys;
			internal volatile V[] values;

			internal volatile int capacity;
			internal volatile int size;
			internal int usedBuckets;
			internal int resizeThreshold;

			internal Section(int capacity)
			{
				this.capacity = alignToPowerOfTwo(capacity);
				this.keys = new long[this.capacity];
				this.values = (V[]) new object[this.capacity];
				this.size = 0;
				this.usedBuckets = 0;
				this.resizeThreshold = (int)(this.capacity * MapFillFactor);
			}

			internal V get(long key, int keyHash)
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
						long storedKey = keys[bucket];
						V storedValue = values[bucket];

						if (!acquiredLock && validate(stamp))
						{
							// The values we have read are consistent
							if (storedKey == key)
							{
								return storedValue != DeletedValue ? storedValue : default(V);
							}
							else if (storedValue == EmptyValue)
							{
								// Not found
								return default(V);
							}
						}
						else
						{
							// Fallback to acquiring read lock
							if (!acquiredLock)
							{
								stamp = readLock();
								acquiredLock = true;
								storedKey = keys[bucket];
								storedValue = values[bucket];
							}

							if (capacity != this.capacity)
							{
								// There has been a rehashing. We need to restart the search
								bucket = keyHash;
								continue;
							}

							if (storedKey == key)
							{
								return storedValue != DeletedValue ? storedValue : default(V);
							}
							else if (storedValue == EmptyValue)
							{
								// Not found
								return default(V);
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

			internal V put(long key, V value, int keyHash, bool onlyIfAbsent, System.Func<long, V> valueProvider)
			{
				int bucket = keyHash;

				long stamp = writeLock();
				int capacity = this.capacity;

				// Remember where we find the first available spot
				int firstDeletedKey = -1;

				try
				{
					while (true)
					{
						bucket = signSafeMod(bucket, capacity);

						long storedKey = keys[bucket];
						V storedValue = values[bucket];

						if (storedKey == key)
						{
							if (storedValue == EmptyValue)
							{
								values[bucket] = value != null ? value : valueProvider(key);
								++size;
								++usedBuckets;
								return valueProvider != null ? values[bucket] : default(V);
							}
							else if (storedValue == DeletedValue)
							{
								values[bucket] = value != null ? value : valueProvider(key);
								++size;
								return valueProvider != null ? values[bucket] : default(V);
							}
							else if (!onlyIfAbsent)
							{
								// Over written an old value for same key
								values[bucket] = value;
								return storedValue;
							}
							else
							{
								return storedValue;
							}
						}
						else if (storedValue == EmptyValue)
						{
							// Found an empty bucket. This means the key is not in the map. If we've already seen a deleted
							// key, we should write at that position
							if (firstDeletedKey != -1)
							{
								bucket = firstDeletedKey;
							}
							else
							{
								++usedBuckets;
							}

							keys[bucket] = key;
							values[bucket] = value != null ? value : valueProvider(key);
							++size;
							return valueProvider != null ? values[bucket] : default(V);
						}
						else if (storedValue == DeletedValue)
						{
							// The bucket contained a different deleted key
							if (firstDeletedKey == -1)
							{
								firstDeletedKey = bucket;
							}
						}

						++bucket;
					}
				}
				finally
				{
					if (usedBuckets >= resizeThreshold)
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

			internal V remove(long key, object value, int keyHash)
			{
				int bucket = keyHash;
				long stamp = writeLock();

				try
				{
					while (true)
					{
						int capacity = this.capacity;
						bucket = signSafeMod(bucket, capacity);

						long storedKey = keys[bucket];
						V storedValue = values[bucket];
						if (storedKey == key)
						{
							if (value == null || value.Equals(storedValue))
							{
								if (storedValue == EmptyValue || storedValue == DeletedValue)
								{
									return default(V);
								}

								--size;
								V nextValueInArray = values[signSafeMod(bucket + 1, capacity)];
								if (nextValueInArray == EmptyValue)
								{
									values[bucket] = (V) EmptyValue;
									--usedBuckets;
								}
								else
								{
									values[bucket] = (V) DeletedValue;
								}

								return storedValue;
							}
							else
							{
								return default(V);
							}
						}
						else if (storedValue == EmptyValue)
						{
							// Key wasn't found
							return default(V);
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
					Arrays.fill(keys, 0);
					Arrays.fill(values, EmptyValue);
					this.size = 0;
					this.usedBuckets = 0;
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

			public void forEach(EntryProcessor<V> processor)
			{
				long stamp = tryOptimisticRead();

				int capacity = this.capacity;
				long[] keys = this.keys;
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
						keys = this.keys;
						values = this.values;
					}

					// Go through all the buckets for this section
					for (int bucket = 0; bucket < capacity; bucket++)
					{
						long storedKey = keys[bucket];
						V storedValue = values[bucket];

						if (!acquiredReadLock && !validate(stamp))
						{
							// Fallback to acquiring read lock
							stamp = readLock();
							acquiredReadLock = true;

							storedKey = keys[bucket];
							storedValue = values[bucket];
						}

						if (storedValue != DeletedValue && storedValue != EmptyValue)
						{
							processor.accept(storedKey, storedValue);
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
				long[] newKeys = new long[newCapacity];
				V[] newValues = (V[]) new object[newCapacity];

				// Re-hash table
				for (int i = 0; i < keys.Length; i++)
				{
					long storedKey = keys[i];
					V storedValue = values[i];
					if (storedValue != EmptyValue && storedValue != DeletedValue)
					{
						insertKeyValueNoLock(newKeys, newValues, storedKey, storedValue);
					}
				}

				keys = newKeys;
				values = newValues;
				capacity = newCapacity;
				usedBuckets = size;
				resizeThreshold = (int)(capacity * MapFillFactor);
			}

			internal static void insertKeyValueNoLock<V>(long[] keys, V[] values, long key, V value)
			{
				int bucket = (int) hash(key);

				while (true)
				{
					bucket = signSafeMod(bucket, keys.Length);

					V storedValue = values[bucket];

					if (storedValue == EmptyValue)
					{
						// The bucket is empty, so we can use it
						keys[bucket] = key;
						values[bucket] = value;
						return;
					}

					++bucket;
				}
			}
		}

		private const long HashMixer = unchecked((long)0xc6a4a7935bd1e995L);
		private const int R = 47;

		internal static long hash(long key)
		{
			long hash = key * HashMixer;
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