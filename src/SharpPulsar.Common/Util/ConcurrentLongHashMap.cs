
using System;
using System.Collections.Generic;
using SharpPulsar.Common.Precondition;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Common.Util
{
    //using static System.Collections.Specialized.BitVector32;

    /// <summary>
    /// Map from long to an Object.
    /// 
    /// <para>Provides similar methods as a {@code ConcurrentMap<long,Object>} with 2 differences:
    /// <ol>
    /// <li>No boxing/unboxing from long -> Long
    /// <li>Open hash map with linear probing, no node allocations to store the values
    /// </ol>
    /// 
    /// <b>WARN: method forEach do not guarantee thread safety, nor do the keys and values method.</b>
    /// <br>
    /// The forEach method is specifically designed for single-threaded usage. When iterating over a map
    /// with concurrent writes, it becomes possible for new values to be either observed or not observed.
    /// There is no guarantee that if we write value1 and value2, and are able to see value2, then we will also see value1.
    /// In some cases, it is even possible to encounter two mappings with the same key,
    /// leading the keys method to return a List containing two identical keys.
    /// 
    /// <br>
    /// It is crucial to understand that the results obtained from aggregate status methods such as keys and values
    /// are typically reliable only when the map is not undergoing concurrent updates from other threads.
    /// When concurrent updates are involved, the results of these methods reflect transient states
    /// that may be suitable for monitoring or estimation purposes, but not for program control.
    /// </para>
    /// </summary>
    /// @param <V> </param>
    public class ConcurrentLongHashMap<V>
    {

        private const object EmptyValue = null;
        private static readonly object DeletedValue = new object();

        private const int DefaultExpectedItems = 256;
        private const int DefaultConcurrencyLevel = 16;

        private const float DefaultMapFillFactor = 0.66f;
        private const float DefaultMapIdleFactor = 0.15f;

        private const float DefaultExpandFactor = 2;
        private const float DefaultShrinkFactor = 2;

        private const bool DefaultAutoShrink = false;

        public static Builder<V> NewBuilder<V>()
        {
            return new Builder<V>();
        }

        /// <summary>
        /// Builder of ConcurrentLongHashMap.
        /// </summary>
        public class Builder<T>
        {
            private int _expectedItems = DefaultExpectedItems;
            private int _concurrencyLevel = DefaultConcurrencyLevel;
            private float _mapFillFactor = DefaultMapFillFactor;
            private float _mapIdleFactor = DefaultMapIdleFactor;
            private float _expandFactor = DefaultExpandFactor;
            private float _shrinkFactor = DefaultShrinkFactor;
            private bool _autoShrink = DefaultAutoShrink;

            public virtual Builder<T> ExpectedItems(int expectedItems)
            {
                _expectedItems = expectedItems;
                return this;
            }

            public virtual Builder<T> ConcurrencyLevel(int concurrencyLevel)
            {
                _concurrencyLevel = concurrencyLevel;
                return this;
            }

            public virtual Builder<T> MapFillFactor(float mapFillFactor)
            {
                _mapFillFactor = mapFillFactor;
                return this;
            }

            public virtual Builder<T> MapIdleFactor(float mapIdleFactor)
            {
                _mapIdleFactor = mapIdleFactor;
                return this;
            }

            public virtual Builder<T> ExpandFactor(float expandFactor)
            {
                _expandFactor = expandFactor;
                return this;
            }

            public virtual Builder<T> ShrinkFactor(float shrinkFactor)
            {
                _shrinkFactor = shrinkFactor;
                return this;
            }

            public virtual Builder<T> AutoShrink(bool autoShrink)
            {
                _autoShrink = autoShrink;
                return this;
            }

            public virtual ConcurrentLongHashMap<T> Build()
            {
                return new ConcurrentLongHashMap<T>(_expectedItems, _concurrencyLevel, _mapFillFactor, _mapIdleFactor, _autoShrink, _expandFactor, _shrinkFactor);
            }
        }

        private readonly Section<V>[] sections;

        [Obsolete]
        public ConcurrentLongHashMap() : this(DefaultExpectedItems)
        {
        }

        [Obsolete]
        public ConcurrentLongHashMap(int expectedItems) : this(expectedItems, DefaultConcurrencyLevel)
        {
        }

        [Obsolete]
        public ConcurrentLongHashMap(int expectedItems, int concurrencyLevel) : this(expectedItems, concurrencyLevel, DefaultMapFillFactor, DefaultMapIdleFactor, DefaultAutoShrink, DefaultExpandFactor, DefaultShrinkFactor)
        {
        }

        public ConcurrentLongHashMap(int expectedItems, int concurrencyLevel, float mapFillFactor, float mapIdleFactor, bool autoShrink, float expandFactor, float shrinkFactor)
        {
            Condition.CheckArgument(expectedItems > 0);
            Condition.CheckArgument(concurrencyLevel > 0);
            Condition.CheckArgument(expectedItems >= concurrencyLevel);
            Condition.CheckArgument(mapFillFactor > 0 && mapFillFactor < 1);
            Condition.CheckArgument(mapIdleFactor > 0 && mapIdleFactor < 1);
            Condition.CheckArgument(mapFillFactor > mapIdleFactor);
            Condition.CheckArgument(expandFactor > 1);
            Condition.CheckArgument(shrinkFactor > 1);

            var numSections = concurrencyLevel;
            var perSectionExpectedItems = expectedItems / numSections;
            var perSectionCapacity = (int)(perSectionExpectedItems / mapFillFactor);
            sections = (Section<V>[])new Section[numSections];

            for (var i = 0; i < numSections; i++)
            {
                sections[i] = new Section<V>(perSectionCapacity, mapFillFactor, mapIdleFactor, autoShrink, expandFactor, shrinkFactor);
            }
        }

        public virtual long Size()
        {
            long size = 0;
            foreach (var s in sections)
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
                foreach (var s in sections)
                {
                    usedBucketCount += s.usedBuckets;
                }
                return usedBucketCount;
            }
        }

        public virtual long Capacity()
        {
            long capacity = 0;
            foreach (var s in sections)
            {
                capacity += s.capacity;
            }
            return capacity;
        }

        public virtual bool Empty
        {
            get
            {
                foreach (var s in sections)
                {
                    if (s.size != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public virtual V Get(long key)
        {
            var h = Hash(key);
            return GetSection(h).Get(key, (int)h);
        }

        public virtual bool ContainsKey(long key)
        {
            return Get(key) != null;
        }

        public virtual V Put(long key, V value)
        {
            RequireNonNull(value);
            var h = Hash(key);
            return GetSection(h).Put(key, value, (int)h, false, null);
        }

        public virtual V PutIfAbsent(long key, V value)
        {
            requireNonNull(value);
            var h = Hash(key);
            return GetSection(h).Put(key, value, (int)h, true, null);
        }

        public virtual V ComputeIfAbsent(long key, Func<long, V> provider)
        {
            requireNonNull(provider);
            var h = Hash(key);
            return GetSection(h).Put(key, default, (int)h, true, provider);
        }

        public virtual V Remove(long key)
        {
            var h = Hash(key);
            return GetSection(h).Remove(key, null, (int)h);
        }

        public virtual bool Remove(long key, object value)
        {
            Condition.RequireNonNull(value);
            var h = Hash(key);
            return GetSection(h).Remove(key, value, (int)h) != null;
        }

        private Section<V> GetSection(long hash)
        {
            // Use 32 msb out of long to get the section;
            var sectionIdx = (int)(long)((ulong)hash >> 32) & sections.Length - 1;
            return sections[sectionIdx];
        }

        public virtual void Clear()
        {
            for (var i = 0; i < sections.Length; i++)
            {
                sections[i].Clear();
            }
        }

        /// <summary>
        /// Iterate over all the entries in the map and apply the processor function to each of them.
        /// <para>
        /// <b>Warning: Do Not Guarantee Thread-Safety.</b>
        /// </para>
        /// </summary>
        /// <param name="processor"> the processor to apply to each entry </param>
        public virtual void ForEach(EntryProcessor<V> processor)
        {
            for (var i = 0; i < sections.Length; i++)
            {
                sections[i].ForEach(processor);
            }
        }

        /// <returns> a new list of all keys (makes a copy) </returns>
        public virtual IList<long> Keys()
        {
            IList<long> keys = new List<long>((int)Size());
            ForEach((key, value => keys.Add(key));
            return keys;
        }

        public virtual IList<V> Values()
        {
            IList<V> values = new List<V>((int)Size()); ;
            ForEach((key, value) => values.Add(value));
            return values;
        }

        /// <summary>
        /// Processor for one key-value entry, where the key is {@code long}.
        /// </summary>
        /// @param <V> type of the value. </param>
        public interface EntryProcessor<V>
        {
            void Accept(long key, V value);
        }

        // A section is a portion of the hash map that is covered by a single
        private sealed class Section<S> 
        {
            internal volatile long[] keys;
            internal volatile S[] values;

            internal volatile int capacity;
            internal readonly int initCapacity;

            internal volatile int size;
            internal int usedBuckets;
            internal int resizeThresholdUp;
            internal int resizeThresholdBelow;
            internal readonly float mapFillFactor;
            internal readonly float mapIdleFactor;
            internal readonly float expandFactor;
            internal readonly float shrinkFactor;
            internal readonly bool autoShrink;

            internal Section(int capacity, float mapFillFactor, float mapIdleFactor, bool autoShrink, float expandFactor, float shrinkFactor)
            {
                this.capacity = AlignToPowerOfTwo(capacity);
                initCapacity = this.capacity;
                keys = new long[this.capacity];
                values = (S[])new object[this.capacity];
                size = 0;
                usedBuckets = 0;
                this.autoShrink = autoShrink;
                this.mapFillFactor = mapFillFactor;
                this.mapIdleFactor = mapIdleFactor;
                this.expandFactor = expandFactor;
                this.shrinkFactor = shrinkFactor;
                resizeThresholdUp = (int)(this.capacity * mapFillFactor);
                resizeThresholdBelow = (int)(this.capacity * mapIdleFactor);
            }

            internal S Get(long key, int keyHash)
            {
                long stamp = TryOptimisticRead();
                var acquiredLock = false;

                // add local variable here, so OutOfBound won't happen
                var keys = this.keys;
                var values = this.values;
                // calculate table.length as capacity to avoid rehash changing capacity
                var bucket = SignSafeMod(keyHash, values.Length);

                try
                {
                    while (true)
                    {
                        // First try optimistic locking
                        var storedKey = keys[bucket];
                        var storedValue = values[bucket];

                        if (!acquiredLock && Validate(stamp))
                        {
                            // The values we have read are consistent
                            if (storedKey == key)
                            {
                                return storedValue != DeletedValue ? storedValue : default;
                            }
                            else if (storedValue == EmptyValue)
                            {
                                // Not found
                                return default;
                            }
                        }
                        else
                        {
                            // Fallback to acquiring read lock
                            if (!acquiredLock)
                            {
                                stamp = ReadLock();
                                acquiredLock = true;

                                // update local variable
                                keys = this.keys;
                                values = this.values;
                                bucket = SignSafeMod(keyHash, values.Length);
                                storedKey = keys[bucket];
                                storedValue = values[bucket];
                            }

                            if (storedKey == key)
                            {
                                return storedValue != DeletedValue ? storedValue : default;
                            }
                            else if (storedValue == EmptyValue)
                            {
                                // Not found
                                return default;
                            }
                        }
                        bucket = bucket + 1 & values.Length - 1;
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

            internal V Put(long key, V value, int keyHash, bool onlyIfAbsent, Func<long, V> valueProvider)
            {
                var bucket = keyHash;

                long stamp = WriteLock();
                var capacity = this.capacity;

                // Remember where we find the first available spot
                var firstDeletedKey = -1;

                try
                {
                    while (true)
                    {
                        bucket = SignSafeMod(bucket, capacity);

                        var storedKey = keys[bucket];
                        var storedValue = values[bucket];

                        if (storedKey == key)
                        {
                            if (storedValue == EmptyValue)
                            {
                                values[bucket] = value != null ? value : valueProvider(key);
                                SIZE_UPDATER.incrementAndGet(this);
                                ++usedBuckets;
                                return valueProvider != null ? values[bucket] : default;
                            }
                            else if (storedValue == DeletedValue)
                            {
                                values[bucket] = value != null ? value : valueProvider(key);
                                SIZE_UPDATER.incrementAndGet(this);
                                return valueProvider != null ? values[bucket] : default;
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
                            SIZE_UPDATER.incrementAndGet(this);
                            return valueProvider != null ? values[bucket] : default;
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
                    if (usedBuckets > resizeThresholdUp)
                    {
                        try
                        {
                            var newCapacity = AlignToPowerOfTwo((int)(capacity * expandFactor));
                            Rehash(newCapacity);
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

            internal V Remove(long key, object value, int keyHash)
            {
                var bucket = keyHash;
                long stamp = writeLock();

                try
                {
                    while (true)
                    {
                        var capacity = this.capacity;
                        bucket = SignSafeMod(bucket, capacity);

                        var storedKey = keys[bucket];
                        var storedValue = values[bucket];
                        if (storedKey == key)
                        {
                            if (value == null || value.Equals(storedValue))
                            {
                                if (storedValue == EmptyValue || storedValue == DeletedValue)
                                {
                                    return default;
                                }

                                SIZE_UPDATER.decrementAndGet(this);
                                var nextValueInArray = values[SignSafeMod(bucket + 1, capacity)];
                                if (nextValueInArray == EmptyValue)
                                {
                                    values[bucket] = (V)EmptyValue;
                                    --usedBuckets;

                                    // Cleanup all the buckets that were in `DeletedValue` state,
                                    // so that we can reduce unnecessary expansions
                                    var lastBucket = SignSafeMod(bucket - 1, capacity);
                                    while (values[lastBucket] == DeletedValue)
                                    {
                                        values[lastBucket] = (V)EmptyValue;
                                        --usedBuckets;

                                        lastBucket = SignSafeMod(lastBucket - 1, capacity);
                                    }
                                }
                                else
                                {
                                    values[bucket] = (V)DeletedValue;
                                }

                                return storedValue;
                            }
                            else
                            {
                                return default;
                            }
                        }
                        else if (storedValue == EmptyValue)
                        {
                            // Key wasn't found
                            return default;
                        }

                        ++bucket;
                    }

                }
                finally
                {
                    if (autoShrink && size < resizeThresholdBelow)
                    {
                        try
                        {
                            // Shrinking must at least ensure initCapacity,
                            // so as to avoid frequent shrinking and expansion near initCapacity,
                            // frequent shrinking and expansion,
                            // additionally opened arrays will consume more memory and affect GC
                            var newCapacity = Math.Max(AlignToPowerOfTwo((int)(capacity / shrinkFactor)), initCapacity);
                            var newResizeThresholdUp = (int)(newCapacity * mapFillFactor);
                            if (newCapacity < capacity && newResizeThresholdUp > size)
                            {
                                // shrink the hashmap
                                Rehash(newCapacity);
                            }
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

            internal void Clear()
            {
                long stamp = writeLock();

                try
                {
                    if (autoShrink && capacity > initCapacity)
                    {
                        ShrinkToInitCapacity();
                    }
                    else
                    {
                        Arrays.Fill(keys, 0);
                        Arrays.Fill(values, EmptyValue);
                        size = 0;
                        usedBuckets = 0;
                    }
                }
                finally
                {
                    unlockWrite(stamp);
                }
            }

            public void ForEach(EntryProcessor<V> processor)
            {
                long stamp = tryOptimisticRead();

                // We need to make sure that we read these 3 variables in a consistent way
                var capacity = this.capacity;
                var keys = this.keys;
                var values = this.values;

                // Validate no rehashing
                if (!validate(stamp))
                {
                    // Fallback to read lock
                    stamp = readLock();

                    capacity = this.capacity;
                    keys = this.keys;
                    values = this.values;
                    unlockRead(stamp);
                }

                // Go through all the buckets for this section. We try to renew the stamp only after a validation
                // error, otherwise we keep going with the same.
                for (var bucket = 0; bucket < capacity; bucket++)
                {
                    if (stamp == 0)
                    {
                        stamp = tryOptimisticRead();
                    }

                    var storedKey = keys[bucket];
                    var storedValue = values[bucket];

                    if (!validate(stamp))
                    {
                        // Fallback to acquiring read lock
                        stamp = readLock();

                        try
                        {
                            storedKey = keys[bucket];
                            storedValue = values[bucket];
                        }
                        finally
                        {
                            unlockRead(stamp);
                        }

                        stamp = 0;
                    }

                    if (storedValue != DeletedValue && storedValue != EmptyValue)
                    {
                        processor.accept(storedKey, storedValue);
                    }
                }
            }

            internal void Rehash(int newCapacity)
            {
                // Expand the hashmap
                var newKeys = new long[newCapacity];
                var newValues = (V[])new object[newCapacity];

                // Re-hash table
                for (var i = 0; i < keys.Length; i++)
                {
                    var storedKey = keys[i];
                    var storedValue = values[i];
                    if (storedValue != EmptyValue && storedValue != DeletedValue)
                    {
                        InsertKeyValueNoLock(newKeys, newValues, storedKey, storedValue);
                    }
                }

                keys = newKeys;
                values = newValues;
                capacity = newCapacity;
                usedBuckets = size;
                resizeThresholdUp = (int)(capacity * mapFillFactor);
                resizeThresholdBelow = (int)(capacity * mapIdleFactor);
            }

            internal void ShrinkToInitCapacity()
            {
                var newKeys = new long[initCapacity];
                var newValues = (V[])new object[initCapacity];

                keys = newKeys;
                values = newValues;
                size = 0;
                usedBuckets = 0;
                // Capacity needs to be updated after the values, so that we won't see
                // a capacity value bigger than the actual array size
                capacity = initCapacity;
                resizeThresholdUp = (int)(capacity * mapFillFactor);
                resizeThresholdBelow = (int)(capacity * mapIdleFactor);
            }

            internal static void InsertKeyValueNoLock<V>(long[] keys, V[] values, long key, V value)
            {
                var bucket = (int)Hash(key);

                while (true)
                {
                    bucket = SignSafeMod(bucket, keys.Length);

                    var storedValue = values[bucket];

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

        internal static long Hash(long key)
        {
            var hash = key * HashMixer;
            hash ^= (long)((ulong)hash >> R);
            hash *= HashMixer;
            return hash;
        }

        internal static int SignSafeMod(long n, int max)
        {
            return (int)n & max - 1;
        }

        private static int AlignToPowerOfTwo(int n)
        {
            return (int)Math.Pow(2, 32 - Integer.numberOfLeadingZeros(n - 1));
        }
    }
}


internal static class Arrays
{
    public static T[] CopyOf<T>(T[] original, int newLength)
    {
        var dest = new T[newLength];
        Array.Copy(original, dest, Math.Min(original.Length, newLength));
        return dest;
    }

    public static T[] CopyOfRange<T>(T[] original, int fromIndex, int toIndex)
    {
        var length = toIndex - fromIndex;
        var dest = new T[length];
        Array.Copy(original, fromIndex, dest, 0, length);
        return dest;
    }

    public static void Fill<T>(T[] array, T value)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
    {
        for (var i = fromIndex; i < toIndex; i++)
        {
            array[i] = value;
        }
    }
}
