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
namespace SharpPulsar.Util.Collections
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Lists = com.google.common.collect.Lists;

	/// <summary>
	/// Concurrent hash map.
	/// 
	/// <para>Provides similar methods as a {@code ConcurrentMap<K,V>} but since it's an open hash map with linear probing,
	/// no node allocations are required to store the values.
	/// 
	/// </para>
	/// </summary>
	/// @param <V> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class ConcurrentOpenHashMap<K, V>
	public class ConcurrentOpenHashMap<K, V>
	{

		private const object EmptyKey = null;
		private static readonly object DeletedKey = new object();

		private const float MapFillFactor = 0.66f;

		private const int DefaultExpectedItems = 256;
		private const int DefaultConcurrencyLevel = 16;

		private readonly Section<K, V>[] sections;

		public ConcurrentOpenHashMap() : this(DefaultExpectedItems)
		{
		}

		public ConcurrentOpenHashMap(int expectedItems) : this(expectedItems, DefaultConcurrencyLevel)
		{
		}

		public ConcurrentOpenHashMap(int expectedItems, int concurrencyLevel)
		{
			checkArgument(expectedItems > 0);
			checkArgument(concurrencyLevel > 0);
			checkArgument(expectedItems >= concurrencyLevel);

			int numSections = concurrencyLevel;
			int perSectionExpectedItems = expectedItems / numSections;
			int perSectionCapacity = (int)(perSectionExpectedItems / MapFillFactor);
			this.sections = (Section<K, V>[]) new Section[numSections];

			for (int i = 0; i < numSections; i++)
			{
				sections[i] = new Section<K, V>(perSectionCapacity);
			}
		}

		public virtual long size()
		{
			long size = 0;
			foreach (Section<K, V> s in sections)
			{
				size += s.size;
			}
			return size;
		}

		public virtual long capacity()
		{
			long capacity = 0;
			foreach (Section<K, V> s in sections)
			{
				capacity += s.capacity;
			}
			return capacity;
		}

		public virtual bool Empty
		{
			get
			{
				foreach (Section<K, V> s in sections)
				{
					if (s.size != 0)
					{
						return false;
					}
				}
    
				return true;
			}
		}

		public virtual V get(K key)
		{
			checkNotNull(key);
			long h = hash(key);
			return getSection(h).get(key, (int) h);
		}

		public virtual bool containsKey(K key)
		{
			return get(key) != null;
		}

		public virtual V put(K key, V value)
		{
			checkNotNull(key);
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).put(key, value, (int) h, false, null);
		}

		public virtual V putIfAbsent(K key, V value)
		{
			checkNotNull(key);
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).put(key, value, (int) h, true, null);
		}

		public virtual V computeIfAbsent(K key, System.Func<K, V> provider)
		{
			checkNotNull(key);
			checkNotNull(provider);
			long h = hash(key);
			return getSection(h).put(key, default(V), (int) h, true, provider);
		}

		public virtual V remove(K key)
		{
			checkNotNull(key);
			long h = hash(key);
			return getSection(h).remove(key, null, (int) h);
		}

		public virtual bool remove(K key, object value)
		{
			checkNotNull(key);
			checkNotNull(value);
			long h = hash(key);
			return getSection(h).remove(key, value, (int) h) != null;
		}

		private Section<K, V> getSection(long hash)
		{
			// Use 32 msb out of long to get the section
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sectionIdx = (int)(hash >>> 32) & (sections.length - 1);
			int sectionIdx = (int)((long)((ulong)hash >> 32)) & (sections.Length - 1);
			return sections[sectionIdx];
		}

		public virtual void clear()
		{
			foreach (Section<K, V> s in sections)
			{
				s.clear();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEach(java.util.function.BiConsumer<? super K, ? super V> processor)
		public virtual void forEach<T1>(System.Action<T1> processor)
		{
			foreach (Section<K, V> s in sections)
			{
				s.forEach(processor);
			}
		}

		/// <returns> a new list of all keys (makes a copy) </returns>
		public virtual IList<K> keys()
		{
			IList<K> keys = Lists.newArrayList();
			forEach((key, value) => keys.Add(key));
			return keys;
		}

		public virtual IList<V> values()
		{
			IList<V> values = Lists.newArrayList();
			forEach((key, value) => values.Add(value));
			return values;
		}

		// A section is a portion of the hash map that is covered by a single
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static final class Section<K, V> extends java.util.concurrent.locks.StampedLock
		private sealed class Section<K, V> : StampedLock
		{
			// Keys and values are stored interleaved in the table array
			internal volatile object[] table;

			internal volatile int capacity;
			internal volatile int size;
			internal int usedBuckets;
			internal int resizeThreshold;

			internal Section(int capacity)
			{
				this.capacity = alignToPowerOfTwo(capacity);
				this.table = new object[2 * this.capacity];
				this.size = 0;
				this.usedBuckets = 0;
				this.resizeThreshold = (int)(this.capacity * MapFillFactor);
			}

			internal V get(K key, int keyHash)
			{
				long stamp = tryOptimisticRead();
				bool acquiredLock = false;
				int bucket = signSafeMod(keyHash, capacity);

				try
				{
					while (true)
					{
						// First try optimistic locking
						K storedKey = (K) table[bucket];
						V storedValue = (V) table[bucket + 1];

						if (!acquiredLock && validate(stamp))
						{
							// The values we have read are consistent
							if (key.Equals(storedKey))
							{
								return storedValue;
							}
							else if (storedKey == EmptyKey)
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

								bucket = signSafeMod(keyHash, capacity);
								storedKey = (K) table[bucket];
								storedValue = (V) table[bucket + 1];
							}

							if (key.Equals(storedKey))
							{
								return storedValue;
							}
							else if (storedKey == EmptyKey)
							{
								// Not found
								return default(V);
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

			internal V put(K key, V value, int keyHash, bool onlyIfAbsent, System.Func<K, V> valueProvider)
			{
				long stamp = writeLock();
				int bucket = signSafeMod(keyHash, capacity);

				// Remember where we find the first available spot
				int firstDeletedKey = -1;

				try
				{
					while (true)
					{
						K storedKey = (K) table[bucket];
						V storedValue = (V) table[bucket + 1];

						if (key.Equals(storedKey))
						{
							if (!onlyIfAbsent)
							{
								// Over written an old value for same key
								table[bucket + 1] = value;
								return storedValue;
							}
							else
							{
								return storedValue;
							}
						}
						else if (storedKey == EmptyKey)
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

							if (value == null)
							{
								value = valueProvider(key);
							}

							table[bucket] = key;
							table[bucket + 1] = value;
							++size;
							return valueProvider != null ? value : default(V);
						}
						else if (storedKey == DeletedKey)
						{
							// The bucket contained a different deleted key
							if (firstDeletedKey == -1)
							{
								firstDeletedKey = bucket;
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

			internal V remove(K key, object value, int keyHash)
			{
				long stamp = writeLock();
				int bucket = signSafeMod(keyHash, capacity);

				try
				{
					while (true)
					{
						K storedKey = (K) table[bucket];
						V storedValue = (V) table[bucket + 1];
						if (key.Equals(storedKey))
						{
							if (value == null || value.Equals(storedValue))
							{
								--size;

								int nextInArray = (bucket + 2) & (table.Length - 1);
								if (table[nextInArray] == EmptyKey)
								{
									table[bucket] = EmptyKey;
									table[bucket + 1] = null;
									--usedBuckets;
								}
								else
								{
									table[bucket] = DeletedKey;
									table[bucket + 1] = null;
								}

								return storedValue;
							}
							else
							{
								return default(V);
							}
						}
						else if (storedKey == EmptyKey)
						{
							// Key wasn't found
							return default(V);
						}

						bucket = (bucket + 2) & (table.Length - 1);
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
					Arrays.fill(table, EmptyKey);
					this.size = 0;
					this.usedBuckets = 0;
				}
				finally
				{
					unlockWrite(stamp);
				}
			}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEach(java.util.function.BiConsumer<? super K, ? super V> processor)
			public void forEach<T1>(System.Action<T1> processor)
			{
				long stamp = tryOptimisticRead();

				object[] table = this.table;
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
						K storedKey = (K) table[bucket];
						V storedValue = (V) table[bucket + 1];

						if (!acquiredReadLock && !validate(stamp))
						{
							// Fallback to acquiring read lock
							stamp = readLock();
							acquiredReadLock = true;

							storedKey = (K) table[bucket];
							storedValue = (V) table[bucket + 1];
						}

						if (storedKey != DeletedKey && storedKey != EmptyKey)
						{
							processor(storedKey, storedValue);
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
				object[] newTable = new object[2 * newCapacity];

				// Re-hash table
				for (int i = 0; i < table.Length; i += 2)
				{
					K storedKey = (K) table[i];
					V storedValue = (V) table[i + 1];
					if (storedKey != EmptyKey && storedKey != DeletedKey)
					{
						insertKeyValueNoLock(newTable, newCapacity, storedKey, storedValue);
					}
				}

				table = newTable;
				capacity = newCapacity;
				usedBuckets = size;
				resizeThreshold = (int)(capacity * MapFillFactor);
			}

			internal static void insertKeyValueNoLock<K, V>(object[] table, int capacity, K key, V value)
			{
				int bucket = signSafeMod(hash(key), capacity);

				while (true)
				{
					K storedKey = (K) table[bucket];

					if (storedKey == EmptyKey)
					{
						// The bucket is empty, so we can use it
						table[bucket] = key;
						table[bucket + 1] = value;
						return;
					}

					bucket = (bucket + 2) & (table.Length - 1);
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
			return (int)(n & (max - 1)) << 1;
		}

		private static int alignToPowerOfTwo(int n)
		{
			return (int) Math.Pow(2, 32 - Integer.numberOfLeadingZeros(n - 1));
		}
	}

}