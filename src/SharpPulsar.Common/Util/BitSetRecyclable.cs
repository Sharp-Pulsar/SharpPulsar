using System;
using System.Diagnostics;
using System.Text;

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
    /// <summary>
    /// This this copy of <seealso cref="System.Collections.BitArray"/>.
    /// Provides <seealso cref="BitSetRecyclable.resetWords(long[])"/> method and leverage with netty recycler.
    /// </summary>
    [Serializable]
    public class BitSetRecyclable : ICloneable
    {
        /*
		 * BitSets are packed into arrays of "words."  Currently a word is
		 * a long, which consists of 64 bits, requiring 6 address bits.
		 * The choice of word size is determined purely by performance concerns.
		 */
        private const int ADDRESS_BITS_PER_WORD = 6;
        private static readonly int BITS_PER_WORD = 1 << ADDRESS_BITS_PER_WORD;
        private static readonly int BIT_INDEX_MASK = BITS_PER_WORD - 1;

        /* Used to shift left or right for a partial word mask */
        private const long WORD_MASK = unchecked((long)0xffffffffffffffffL);

        /// <summary>
        /// @serialField bits long[]
        /// 
        /// The bits in this BitSet.  The ith bit is stored in bits[i/64] at
        /// bit position i % 64 (where bit position 0 refers to the least
        /// significant bit and 63 refers to the most significant bit).
        /// </summary>
        private static readonly ObjectStreamField[] serialPersistentFields = new ObjectStreamField[] { new ObjectStreamField("bits", typeof(long[])) };

        /// <summary>
        /// The internal field corresponding to the serialField "bits".
        /// </summary>
        private long[] words;

        /// <summary>
        /// The number of words in the logical size of this BitSet.
        /// </summary>
        [NonSerialized]
        private int wordsInUse = 0;

        /// <summary>
        /// Whether the size of "words" is user-specified.  If so, we assume
        /// the user knows what he's doing and try harder to preserve it.
        /// </summary>
        [NonSerialized]
        private bool sizeIsSticky = false;

        /* use serialVersionUID from JDK 1.0.2 for interoperability */
        private const long serialVersionUID = 7997698588986878753L;

        /// <summary>
        /// Given a bit index, return word index containing it.
        /// </summary>
        private static int wordIndex(int bitIndex)
        {
            return bitIndex >> ADDRESS_BITS_PER_WORD;
        }

        /// <summary>
        /// Every public method must preserve these invariants.
        /// </summary>
        private void checkInvariants()
        {
            Debug.Assert((wordsInUse == 0 || words[wordsInUse - 1] != 0));
            Debug.Assert((wordsInUse >= 0 && wordsInUse <= words.Length));
            Debug.Assert((wordsInUse == words.Length || words[wordsInUse] == 0));
        }

        /// <summary>
        /// Sets the field wordsInUse to the logical size in words of the bit set.
        /// WARNING:This method assumes that the number of words actually in use is
        /// less than or equal to the current value of wordsInUse!
        /// </summary>
        private void recalculateWordsInUse()
        {
            // Traverse the bitset until a used word is found
            int i;
            for (i = wordsInUse - 1; i >= 0; i--)
            {
                if (words[i] != 0)
                {
                    break;
                }
            }

            wordsInUse = i + 1; // The new logical size
        }

        /// <summary>
        /// Creates a new bit set. All bits are initially {@code false}.
        /// </summary>
        public BitSetRecyclable()
        {
            initWords(BITS_PER_WORD);
            sizeIsSticky = false;
        }

        /// <summary>
        /// Creates a bit set whose initial size is large enough to explicitly
        /// represent bits with indices in the range {@code 0} through
        /// {@code nbits-1}. All bits are initially {@code false}.
        /// </summary>
        /// <param name="nbits"> the initial size of the bit set </param>
        /// <exception cref="NegativeArraySizeException"> if the specified initial size
        ///         is negative </exception>
        public BitSetRecyclable(int nbits)
        {
            // nbits can't be negative; size 0 is OK
            if (nbits < 0)
            {
                throw new NegativeArraySizeException("nbits < 0: " + nbits);
            }

            initWords(nbits);
            sizeIsSticky = true;
        }

        private void initWords(int nbits)
        {
            words = new long[wordIndex(nbits - 1) + 1];
        }

        /// <summary>
        /// Creates a bit set using words as the internal representation.
        /// The last word (if there is one) must be non-zero.
        /// </summary>
        private BitSetRecyclable(long[] words)
        {
            this.words = words;
            this.wordsInUse = words.Length;
            checkInvariants();
        }

        /// <summary>
        /// Returns a new bit set containing all the bits in the given long array.
        /// 
        /// <para>More precisely,
        /// <br>{@code BitSet.valueOf(longs).get(n) == ((longs[n/64] & (1L<<(n%64))) != 0)}
        /// <br>for all {@code n < 64 * longs.length}.
        /// 
        /// </para>
        /// <para>This method is equivalent to
        /// {@code BitSet.valueOf(LongBuffer.wrap(longs))}.
        /// 
        /// </para>
        /// </summary>
        /// <param name="longs"> a long array containing a little-endian representation
        ///        of a sequence of bits to be used as the initial bits of the
        ///        new bit set </param>
        /// <returns> a {@code BitSet} containing all the bits in the long array
        /// @since 1.7 </returns>
        public static BitSetRecyclable valueOf(long[] longs)
        {
            int n;
            for (n = longs.Length; n > 0 && longs[n - 1] == 0; n--)
            {
                ;
            }
            return new BitSetRecyclable(Arrays.CopyOf(longs, n));
        }

        /// <summary>
        /// Returns a new bit set containing all the bits in the given long
        /// buffer between its position and limit.
        /// 
        /// <para>More precisely,
        /// <br>{@code BitSet.valueOf(lb).get(n) == ((lb.get(lb.position()+n/64) & (1L<<(n%64))) != 0)}
        /// <br>for all {@code n < 64 * lb.remaining()}.
        /// 
        /// </para>
        /// <para>The long buffer is not modified by this method, and no
        /// reference to the buffer is retained by the bit set.
        /// 
        /// </para>
        /// </summary>
        /// <param name="lb"> a long buffer containing a little-endian representation
        ///        of a sequence of bits between its position and limit, to be
        ///        used as the initial bits of the new bit set </param>
        /// <returns> a {@code BitSet} containing all the bits in the buffer in the
        ///         specified range
        /// @since 1.7 </returns>
        public static BitSetRecyclable valueOf(LongBuffer lb)
        {
            lb = lb.slice();
            int n;
            for (n = lb.remaining(); n > 0 && lb.get(n - 1) == 0; n--)
            {
                ;
            }
            long[] words = new long[n];
            lb.get(words);
            return new BitSetRecyclable(words);
        }

        /// <summary>
        /// Returns a new bit set containing all the bits in the given byte array.
        /// 
        /// <para>More precisely,
        /// <br>{@code BitSet.valueOf(bytes).get(n) == ((bytes[n/8] & (1<<(n%8))) != 0)}
        /// <br>for all {@code n <  8 * bytes.length}.
        /// 
        /// </para>
        /// <para>This method is equivalent to
        /// {@code BitSet.valueOf(ByteBuffer.wrap(bytes))}.
        /// 
        /// </para>
        /// </summary>
        /// <param name="bytes"> a byte array containing a little-endian
        ///        representation of a sequence of bits to be used as the
        ///        initial bits of the new bit set </param>
        /// <returns> a {@code BitSet} containing all the bits in the byte array
        /// @since 1.7 </returns>
        public static BitSetRecyclable valueOf(sbyte[] bytes)
        {
            return BitSetRecyclable.valueOf(ByteBuffer.Wrap(bytes));
        }

        /// <summary>
        /// Copy a BitSetRecyclable.
        /// </summary>
        public static BitSetRecyclable valueOf(BitSetRecyclable src)
        {
            // The internal implementation will do the array-copy.
            return valueOf(src.words);
        }

        /// <summary>
        /// Returns a new bit set containing all the bits in the given byte
        /// buffer between its position and limit.
        /// 
        /// <para>More precisely,
        /// <br>{@code BitSet.valueOf(bb).get(n) == ((bb.get(bb.position()+n/8) & (1<<(n%8))) != 0)}
        /// <br>for all {@code n < 8 * bb.remaining()}.
        /// 
        /// </para>
        /// <para>The byte buffer is not modified by this method, and no
        /// reference to the buffer is retained by the bit set.
        /// 
        /// </para>
        /// </summary>
        /// <param name="bb"> a byte buffer containing a little-endian representation
        ///        of a sequence of bits between its position and limit, to be
        ///        used as the initial bits of the new bit set </param>
        /// <returns> a {@code BitSet} containing all the bits in the buffer in the
        ///         specified range
        /// @since 1.7 </returns>
        public static BitSetRecyclable valueOf(ByteBuffer bb)
        {
            bb = bb.slice().order(ByteOrder.LITTLE_ENDIAN);
            int n;
            for (n = bb.remaining(); n > 0 && bb.get(n - 1) == 0; n--)
            {
                ;
            }
            long[] words = new long[(n + 7) / 8];
            bb.limit(n);
            int i = 0;
            while (bb.remaining() >= 8)
            {
                words[i++] = bb.getLong();
            }
            for (int remaining = bb.remaining(), j = 0; j < remaining; j++)
            {
                words[i] |= (bb.get() & 0xffL) << (8 * j);
            }
            return new BitSetRecyclable(words);
        }

        /// <summary>
        /// Returns a new byte array containing all the bits in this bit set.
        /// 
        /// <para>More precisely, if
        /// <br>{@code byte[] bytes = s.toByteArray();}
        /// <br>then {@code bytes.length == (s.length()+7)/8} and
        /// <br>{@code s.get(n) == ((bytes[n/8] & (1<<(n%8))) != 0)}
        /// <br>for all {@code n < 8 * bytes.length}.
        /// 
        /// </para>
        /// </summary>
        /// <returns> a byte array containing a little-endian representation
        ///         of all the bits in this bit set
        /// @since 1.7 </returns>
        public virtual sbyte[] toByteArray()
        {
            int n = wordsInUse;
            if (n == 0)
            {
                return new sbyte[0];
            }
            int len = 8 * (n - 1);
            for (long x = words[n - 1]; x != 0; x = (long)((ulong)x >> 8))
            {
                len++;
            }
            sbyte[] bytes = new sbyte[len];
            ByteBuffer bb = ByteBuffer.Wrap(bytes).order(ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < n - 1; i++)
            {
                bb.putLong(words[i]);
            }
            for (long x = words[n - 1]; x != 0; x = (long)((ulong)x >> 8))
            {
                bb.put(unchecked((sbyte)(x & 0xff)));
            }
            return bytes;
        }

        /// <summary>
        /// Returns a new long array containing all the bits in this bit set.
        /// 
        /// <para>More precisely, if
        /// <br>{@code long[] longs = s.toLongArray();}
        /// <br>then {@code longs.length == (s.length()+63)/64} and
        /// <br>{@code s.get(n) == ((longs[n/64] & (1L<<(n%64))) != 0)}
        /// <br>for all {@code n < 64 * longs.length}.
        /// 
        /// </para>
        /// </summary>
        /// <returns> a long array containing a little-endian representation
        ///         of all the bits in this bit set
        /// @since 1.7 </returns>
        public virtual long[] ToLongArray()
        {
            return Arrays.CopyOf(words, wordsInUse);
        }

        /// <summary>
        /// Ensures that the BitSet can hold enough words. </summary>
        /// <param name="wordsRequired"> the minimum acceptable number of words. </param>
        private void ensureCapacity(int wordsRequired)
        {
            if (words.Length < wordsRequired)
            {
                // Allocate larger of doubled size or required size
                int request = Math.Max(2 * words.Length, wordsRequired);
                words = Arrays.CopyOf(words, request);
                sizeIsSticky = false;
            }
        }

        /// <summary>
        /// Ensures that the BitSet can accommodate a given wordIndex,
        /// temporarily violating the invariants.  The caller must
        /// restore the invariants before returning to the user,
        /// possibly using recalculateWordsInUse(). </summary>
        /// <param name="wordIndex"> the index to be accommodated. </param>
        private void expandTo(int wordIndex)
        {
            int wordsRequired = wordIndex + 1;
            if (wordsInUse < wordsRequired)
            {
                ensureCapacity(wordsRequired);
                wordsInUse = wordsRequired;
            }
        }

        /// <summary>
        /// Checks that fromIndex ... toIndex is a valid range of bit indices.
        /// </summary>
        private static void checkRange(int fromIndex, int toIndex)
        {
            if (fromIndex < 0)
            {
                throw new System.IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            }
            if (toIndex < 0)
            {
                throw new System.IndexOutOfRangeException("toIndex < 0: " + toIndex);
            }
            if (fromIndex > toIndex)
            {
                throw new System.IndexOutOfRangeException("fromIndex: " + fromIndex + " > toIndex: " + toIndex);
            }
        }

        /// <summary>
        /// Sets the bit at the specified index to the complement of its
        /// current value.
        /// </summary>
        /// <param name="bitIndex"> the index of the bit to flip </param>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  1.4 </exception>
        public virtual void flip(int bitIndex)
        {
            if (bitIndex < 0)
            {
                throw new System.IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            }

            int wordIndex = BitSetRecyclable.wordIndex(bitIndex);
            expandTo(wordIndex);

            words[wordIndex] ^= (1L << bitIndex);

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Sets each bit from the specified {@code fromIndex} (inclusive) to the
        /// specified {@code toIndex} (exclusive) to the complement of its current
        /// value.
        /// </summary>
        /// <param name="fromIndex"> index of the first bit to flip </param>
        /// <param name="toIndex"> index after the last bit to flip </param>
        /// <exception cref="IndexOutOfBoundsException"> if {@code fromIndex} is negative,
        ///         or {@code toIndex} is negative, or {@code fromIndex} is
        ///         larger than {@code toIndex}
        /// @since  1.4 </exception>
        public virtual void flip(int fromIndex, int toIndex)
        {
            checkRange(fromIndex, toIndex);

            if (fromIndex == toIndex)
            {
                return;
            }

            int startWordIndex = wordIndex(fromIndex);
            int endWordIndex = wordIndex(toIndex - 1);
            expandTo(endWordIndex);

            long firstWordMask = WORD_MASK << fromIndex;
            long lastWordMask = (long)((ulong)WORD_MASK >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] ^= (firstWordMask & lastWordMask);
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] ^= firstWordMask;

                // Handle intermediate words, if any
                for (int i = startWordIndex + 1; i < endWordIndex; i++)
                {
                    words[i] ^= WORD_MASK;
                }

                // Handle last word
                words[endWordIndex] ^= lastWordMask;
            }

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Sets the bit at the specified index to {@code true}.
        /// </summary>
        /// <param name="bitIndex"> a bit index </param>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  JDK1.0 </exception>
        public virtual void set(int bitIndex)
        {
            if (bitIndex < 0)
            {
                throw new System.IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            }

            int wordIndex = BitSetRecyclable.wordIndex(bitIndex);
            expandTo(wordIndex);

            words[wordIndex] |= (1L << bitIndex); // Restores invariants

            checkInvariants();
        }

        /// <summary>
        /// Sets the bit at the specified index to the specified value.
        /// </summary>
        /// <param name="bitIndex"> a bit index </param>
        /// <param name="value"> a boolean value to set </param>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  1.4 </exception>
        public virtual void set(int bitIndex, bool value)
        {
            if (value)
            {
                set(bitIndex);
            }
            else
            {
                clear(bitIndex);
            }
        }

        /// <summary>
        /// Sets the bits from the specified {@code fromIndex} (inclusive) to the
        /// specified {@code toIndex} (exclusive) to {@code true}.
        /// </summary>
        /// <param name="fromIndex"> index of the first bit to be set </param>
        /// <param name="toIndex"> index after the last bit to be set </param>
        /// <exception cref="IndexOutOfBoundsException"> if {@code fromIndex} is negative,
        ///         or {@code toIndex} is negative, or {@code fromIndex} is
        ///         larger than {@code toIndex}
        /// @since  1.4 </exception>
        public virtual void set(int fromIndex, int toIndex)
        {
            checkRange(fromIndex, toIndex);

            if (fromIndex == toIndex)
            {
                return;
            }

            // Increase capacity if necessary
            int startWordIndex = wordIndex(fromIndex);
            int endWordIndex = wordIndex(toIndex - 1);
            expandTo(endWordIndex);

            long firstWordMask = WORD_MASK << fromIndex;
            long lastWordMask = (long)((ulong)WORD_MASK >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] |= (firstWordMask & lastWordMask);
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] |= firstWordMask;

                // Handle intermediate words, if any
                for (int i = startWordIndex + 1; i < endWordIndex; i++)
                {
                    words[i] = WORD_MASK;
                }

                // Handle last word (restores invariants)
                words[endWordIndex] |= lastWordMask;
            }

            checkInvariants();
        }

        /// <summary>
        /// Sets the bits from the specified {@code fromIndex} (inclusive) to the
        /// specified {@code toIndex} (exclusive) to the specified value.
        /// </summary>
        /// <param name="fromIndex"> index of the first bit to be set </param>
        /// <param name="toIndex"> index after the last bit to be set </param>
        /// <param name="value"> value to set the selected bits to </param>
        /// <exception cref="IndexOutOfBoundsException"> if {@code fromIndex} is negative,
        ///         or {@code toIndex} is negative, or {@code fromIndex} is
        ///         larger than {@code toIndex}
        /// @since  1.4 </exception>
        public virtual void set(int fromIndex, int toIndex, bool value)
        {
            if (value)
            {
                set(fromIndex, toIndex);
            }
            else
            {
                clear(fromIndex, toIndex);
            }
        }

        /// <summary>
        /// Sets the bit specified by the index to {@code false}.
        /// </summary>
        /// <param name="bitIndex"> the index of the bit to be cleared </param>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  JDK1.0 </exception>
        public virtual void clear(int bitIndex)
        {
            if (bitIndex < 0)
            {
                throw new System.IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            }

            int wordIndex = BitSetRecyclable.wordIndex(bitIndex);
            if (wordIndex >= wordsInUse)
            {
                return;
            }

            words[wordIndex] &= ~(1L << bitIndex);

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Sets the bits from the specified {@code fromIndex} (inclusive) to the
        /// specified {@code toIndex} (exclusive) to {@code false}.
        /// </summary>
        /// <param name="fromIndex"> index of the first bit to be cleared </param>
        /// <param name="toIndex"> index after the last bit to be cleared </param>
        /// <exception cref="IndexOutOfBoundsException"> if {@code fromIndex} is negative,
        ///         or {@code toIndex} is negative, or {@code fromIndex} is
        ///         larger than {@code toIndex}
        /// @since  1.4 </exception>
        public virtual void clear(int fromIndex, int toIndex)
        {
            checkRange(fromIndex, toIndex);

            if (fromIndex == toIndex)
            {
                return;
            }

            int startWordIndex = wordIndex(fromIndex);
            if (startWordIndex >= wordsInUse)
            {
                return;
            }

            int endWordIndex = wordIndex(toIndex - 1);
            if (endWordIndex >= wordsInUse)
            {
                toIndex = length();
                endWordIndex = wordsInUse - 1;
            }

            long firstWordMask = WORD_MASK << fromIndex;
            long lastWordMask = (long)((ulong)WORD_MASK >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] &= ~(firstWordMask & lastWordMask);
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] &= ~firstWordMask;

                // Handle intermediate words, if any
                for (int i = startWordIndex + 1; i < endWordIndex; i++)
                {
                    words[i] = 0;
                }

                // Handle last word
                words[endWordIndex] &= ~lastWordMask;
            }

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Sets all of the bits in this BitSet to {@code false}.
        /// 
        /// @since 1.4
        /// </summary>
        public virtual void clear()
        {
            while (wordsInUse > 0)
            {
                words[--wordsInUse] = 0;
            }
        }

        /// <summary>
        /// Returns the value of the bit with the specified index. The value
        /// is {@code true} if the bit with the index {@code bitIndex}
        /// is currently set in this {@code BitSet}; otherwise, the result
        /// is {@code false}.
        /// </summary>
        /// <param name="bitIndex">   the bit index </param>
        /// <returns> the value of the bit with the specified index </returns>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative </exception>
        public virtual bool get(int bitIndex)
        {
            if (bitIndex < 0)
            {
                throw new System.IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            }

            checkInvariants();

            int wordIndex = BitSetRecyclable.wordIndex(bitIndex);
            return (wordIndex < wordsInUse) && ((words[wordIndex] & (1L << bitIndex)) != 0);
        }

        /// <summary>
        /// Returns a new {@code BitSet} composed of bits from this {@code BitSet}
        /// from {@code fromIndex} (inclusive) to {@code toIndex} (exclusive).
        /// </summary>
        /// <param name="fromIndex"> index of the first bit to include </param>
        /// <param name="toIndex"> index after the last bit to include </param>
        /// <returns> a new {@code BitSet} from a range of this {@code BitSet} </returns>
        /// <exception cref="IndexOutOfBoundsException"> if {@code fromIndex} is negative,
        ///         or {@code toIndex} is negative, or {@code fromIndex} is
        ///         larger than {@code toIndex}
        /// @since  1.4 </exception>
        public virtual BitSetRecyclable get(int fromIndex, int toIndex)
        {
            checkRange(fromIndex, toIndex);

            checkInvariants();

            int len = length();

            // If no set bits in range return empty bitset
            if (len <= fromIndex || fromIndex == toIndex)
            {
                return new BitSetRecyclable(0);
            }

            // An optimization
            if (toIndex > len)
            {
                toIndex = len;
            }

            BitSetRecyclable result = new BitSetRecyclable(toIndex - fromIndex);
            int targetWords = wordIndex(toIndex - fromIndex - 1) + 1;
            int sourceIndex = wordIndex(fromIndex);
            bool wordAligned = ((fromIndex & BIT_INDEX_MASK) == 0);

            // Process all words but the last word
            for (int i = 0; i < targetWords - 1; i++, sourceIndex++)
            {
                result.words[i] = wordAligned ? words[sourceIndex] : ((long)((ulong)words[sourceIndex] >> fromIndex)) | (words[sourceIndex + 1] << -fromIndex);
            }

            // Process the last word
            long lastWordMask = (long)((ulong)WORD_MASK >> -toIndex);
            result.words[targetWords - 1] = ((toIndex - 1) & BIT_INDEX_MASK) < (fromIndex & BIT_INDEX_MASK) ? (((long)((ulong)words[sourceIndex] >> fromIndex)) | (words[sourceIndex + 1] & lastWordMask) << -fromIndex) : ((long)((ulong)(words[sourceIndex] & lastWordMask) >> fromIndex));

            // Set wordsInUse correctly
            result.wordsInUse = targetWords;
            result.recalculateWordsInUse();
            result.checkInvariants();

            return result;
        }

        /// <summary>
        /// Returns the index of the first bit that is set to {@code true}
        /// that occurs on or after the specified starting index. If no such
        /// bit exists then {@code -1} is returned.
        /// 
        /// <para>To iterate over the {@code true} bits in a {@code BitSet},
        /// use the following loop:
        /// 
        ///  <pre> {@code
        /// for (int i = bs.nextSetBit(0); i >= 0; i = bs.nextSetBit(i+1)) {
        ///     // operate on index i here
        ///     if (i == Integer.MAX_VALUE) {
        ///         break; // or (i+1) would overflow
        ///     }
        /// }}</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="fromIndex"> the index to start checking from (inclusive) </param>
        /// <returns> the index of the next set bit, or {@code -1} if there
        ///         is no such bit </returns>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  1.4 </exception>
        public virtual int nextSetBit(int fromIndex)
        {
            if (fromIndex < 0)
            {
                throw new System.IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            }

            checkInvariants();

            int u = wordIndex(fromIndex);
            if (u >= wordsInUse)
            {
                return -1;
            }

            long word = words[u] & (WORD_MASK << fromIndex);

            while (true)
            {
                if (word != 0)
                {
                    return (u * BITS_PER_WORD) + Long.numberOfTrailingZeros(word);
                }
                if (++u == wordsInUse)
                {
                    return -1;
                }
                word = words[u];
            }
        }

        /// <summary>
        /// Returns the index of the first bit that is set to {@code false}
        /// that occurs on or after the specified starting index.
        /// </summary>
        /// <param name="fromIndex"> the index to start checking from (inclusive) </param>
        /// <returns> the index of the next clear bit </returns>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
        /// @since  1.4 </exception>
        public virtual int nextClearBit(int fromIndex)
        {
            // Neither spec nor implementation handle bitsets of maximal length.
            // See 4816253.
            if (fromIndex < 0)
            {
                throw new System.IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            }

            checkInvariants();

            int u = wordIndex(fromIndex);
            if (u >= wordsInUse)
            {
                return fromIndex;
            }

            long word = ~words[u] & (WORD_MASK << fromIndex);

            while (true)
            {
                if (word != 0)
                {
                    return (u * BITS_PER_WORD) + Long.numberOfTrailingZeros(word);
                }
                if (++u == wordsInUse)
                {
                    return wordsInUse * BITS_PER_WORD;
                }
                word = ~words[u];
            }
        }

        /// <summary>
        /// Returns the index of the nearest bit that is set to {@code true}
        /// that occurs on or before the specified starting index.
        /// If no such bit exists, or if {@code -1} is given as the
        /// starting index, then {@code -1} is returned.
        /// 
        /// <para>To iterate over the {@code true} bits in a {@code BitSet},
        /// use the following loop:
        /// 
        ///  <pre> {@code
        /// for (int i = bs.length(); (i = bs.previousSetBit(i-1)) >= 0; ) {
        ///     // operate on index i here
        /// }}</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="fromIndex"> the index to start checking from (inclusive) </param>
        /// <returns> the index of the previous set bit, or {@code -1} if there
        ///         is no such bit </returns>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is less
        ///         than {@code -1}
        /// @since  1.7 </exception>
        public virtual int previousSetBit(int fromIndex)
        {
            if (fromIndex < 0)
            {
                if (fromIndex == -1)
                {
                    return -1;
                }
                throw new System.IndexOutOfRangeException("fromIndex < -1: " + fromIndex);
            }

            checkInvariants();

            int u = wordIndex(fromIndex);
            if (u >= wordsInUse)
            {
                return length() - 1;
            }

            long word = words[u] & ((long)((ulong)WORD_MASK >> -(fromIndex + 1)));

            while (true)
            {
                if (word != 0)
                {
                    return (u + 1) * BITS_PER_WORD - 1 - Long.numberOfLeadingZeros(word);
                }
                if (u-- == 0)
                {
                    return -1;
                }
                word = words[u];
            }
        }

        /// <summary>
        /// Returns the index of the nearest bit that is set to {@code false}
        /// that occurs on or before the specified starting index.
        /// If no such bit exists, or if {@code -1} is given as the
        /// starting index, then {@code -1} is returned.
        /// </summary>
        /// <param name="fromIndex"> the index to start checking from (inclusive) </param>
        /// <returns> the index of the previous clear bit, or {@code -1} if there
        ///         is no such bit </returns>
        /// <exception cref="IndexOutOfBoundsException"> if the specified index is less
        ///         than {@code -1}
        /// @since  1.7 </exception>
        public virtual int previousClearBit(int fromIndex)
        {
            if (fromIndex < 0)
            {
                if (fromIndex == -1)
                {
                    return -1;
                }
                throw new System.IndexOutOfRangeException("fromIndex < -1: " + fromIndex);
            }

            checkInvariants();

            int u = wordIndex(fromIndex);
            if (u >= wordsInUse)
            {
                return fromIndex;
            }

            long word = ~words[u] & ((long)((ulong)WORD_MASK >> -(fromIndex + 1)));

            while (true)
            {
                if (word != 0)
                {
                    return (u + 1) * BITS_PER_WORD - 1 - Long.numberOfLeadingZeros(word);
                }
                if (u-- == 0)
                {
                    return -1;
                }
                word = ~words[u];
            }
        }

        /// <summary>
        /// Returns the "logical size" of this {@code BitSet}: the index of
        /// the highest set bit in the {@code BitSet} plus one. Returns zero
        /// if the {@code BitSet} contains no set bits.
        /// </summary>
        /// <returns> the logical size of this {@code BitSet}
        /// @since  1.2 </returns>
        public virtual int length()
        {
            if (wordsInUse == 0)
            {
                return 0;
            }

            return BITS_PER_WORD * (wordsInUse - 1) + (BITS_PER_WORD - Long.numberOfLeadingZeros(words[wordsInUse - 1]));
        }

        /// <summary>
        /// Returns true if this {@code BitSet} contains no bits that are set
        /// to {@code true}.
        /// </summary>
        /// <returns> boolean indicating whether this {@code BitSet} is empty
        /// @since  1.4 </returns>
        public virtual bool Empty
        {
            get
            {
                return wordsInUse == 0;
            }
        }

        /// <summary>
        /// Returns true if the specified {@code BitSet} has any bits set to
        /// {@code true} that are also set to {@code true} in this {@code BitSet}.
        /// </summary>
        /// <param name="set"> {@code BitSet} to intersect with </param>
        /// <returns> boolean indicating whether this {@code BitSet} intersects
        ///         the specified {@code BitSet}
        /// @since  1.4 </returns>
        public virtual bool intersects(BitSetRecyclable set)
        {
            for (int i = Math.Min(wordsInUse, set.wordsInUse) - 1; i >= 0; i--)
            {
                if ((words[i] & set.words[i]) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the number of bits set to {@code true} in this {@code BitSet}.
        /// </summary>
        /// <returns> the number of bits set to {@code true} in this {@code BitSet}
        /// @since  1.4 </returns>
        public virtual int cardinality()
        {
            int sum = 0;
            for (int i = 0; i < wordsInUse; i++)
            {
                sum += Long.bitCount(words[i]);
            }
            return sum;
        }

        /// <summary>
        /// Performs a logical <b>AND</b> of this target bit set with the
        /// argument bit set. This bit set is modified so that each bit in it
        /// has the value {@code true} if and only if it both initially
        /// had the value {@code true} and the corresponding bit in the
        /// bit set argument also had the value {@code true}.
        /// </summary>
        /// <param name="set"> a bit set </param>
        public virtual void and(BitSetRecyclable set)
        {
            if (this == set)
            {
                return;
            }

            while (wordsInUse > set.wordsInUse)
            {
                words[--wordsInUse] = 0;
            }

            // Perform logical AND on words in common
            for (int i = 0; i < wordsInUse; i++)
            {
                words[i] &= set.words[i];
            }

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Performs a logical <b>OR</b> of this bit set with the bit set
        /// argument. This bit set is modified so that a bit in it has the
        /// value {@code true} if and only if it either already had the
        /// value {@code true} or the corresponding bit in the bit set
        /// argument has the value {@code true}.
        /// </summary>
        /// <param name="set"> a bit set </param>
        public virtual void or(BitSetRecyclable set)
        {
            if (this == set)
            {
                return;
            }

            int wordsInCommon = Math.Min(wordsInUse, set.wordsInUse);

            if (wordsInUse < set.wordsInUse)
            {
                ensureCapacity(set.wordsInUse);
                wordsInUse = set.wordsInUse;
            }

            // Perform logical OR on words in common
            for (int i = 0; i < wordsInCommon; i++)
            {
                words[i] |= set.words[i];
            }

            // Copy any remaining words
            if (wordsInCommon < set.wordsInUse)
            {
                Array.Copy(set.words, wordsInCommon, words, wordsInCommon, wordsInUse - wordsInCommon);
            }

            // recalculateWordsInUse() is unnecessary
            checkInvariants();
        }

        /// <summary>
        /// Performs a logical <b>XOR</b> of this bit set with the bit set
        /// argument. This bit set is modified so that a bit in it has the
        /// value {@code true} if and only if one of the following
        /// statements holds:
        /// <ul>
        /// <li>The bit initially has the value {@code true}, and the
        ///     corresponding bit in the argument has the value {@code false}.
        /// <li>The bit initially has the value {@code false}, and the
        ///     corresponding bit in the argument has the value {@code true}.
        /// </ul>
        /// </summary>
        /// <param name="set"> a bit set </param>
        public virtual void xor(BitSetRecyclable set)
        {
            int wordsInCommon = Math.Min(wordsInUse, set.wordsInUse);

            if (wordsInUse < set.wordsInUse)
            {
                ensureCapacity(set.wordsInUse);
                wordsInUse = set.wordsInUse;
            }

            // Perform logical XOR on words in common
            for (int i = 0; i < wordsInCommon; i++)
            {
                words[i] ^= set.words[i];
            }

            // Copy any remaining words
            if (wordsInCommon < set.wordsInUse)
            {
                Array.Copy(set.words, wordsInCommon, words, wordsInCommon, set.wordsInUse - wordsInCommon);
            }

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Clears all of the bits in this {@code BitSet} whose corresponding
        /// bit is set in the specified {@code BitSet}.
        /// </summary>
        /// <param name="set"> the {@code BitSet} with which to mask this
        ///         {@code BitSet}
        /// @since  1.2 </param>
        public virtual void andNot(BitSetRecyclable set)
        {
            // Perform logical (a & !b) on words in common
            for (int i = Math.Min(wordsInUse, set.wordsInUse) - 1; i >= 0; i--)
            {
                words[i] &= ~set.words[i];
            }

            recalculateWordsInUse();
            checkInvariants();
        }

        /// <summary>
        /// Returns the hash code value for this bit set. The hash code depends
        /// only on which bits are set within this {@code BitSet}.
        /// 
        /// <para>The hash code is defined to be the result of the following
        /// calculation:
        ///  <pre> {@code
        /// public int hashCode() {
        ///     long h = 1234;
        ///     long[] words = toLongArray();
        ///     for (int i = words.length; --i >= 0; )
        ///         h ^= words[i] * (i + 1);
        ///     return (int)((h >> 32) ^ h);
        /// }}</pre>
        /// Note that the hash code changes if the set of bits is altered.
        /// 
        /// </para>
        /// </summary>
        /// <returns> the hash code value for this bit set </returns>
        public override int GetHashCode()
        {
            long h = 1234;
            for (int i = wordsInUse; --i >= 0;)
            {
                h ^= words[i] * (i + 1);
            }

            return (int)((h >> 32) ^ h);
        }

        /// <summary>
        /// Returns the number of bits of space actually in use by this
        /// {@code BitSet} to represent bit values.
        /// The maximum element in the set is the size - 1st element.
        /// </summary>
        /// <returns> the number of bits currently in this bit set </returns>
        public virtual int size()
        {
            return words.Length * BITS_PER_WORD;
        }

        /// <summary>
        /// Compares this object against the specified object.
        /// The result is {@code true} if and only if the argument is
        /// not {@code null} and is a {@code Bitset} object that has
        /// exactly the same set of bits set to {@code true} as this bit
        /// set. That is, for every nonnegative {@code int} index {@code k},
        /// <pre>((BitSet)obj).get(k) == this.get(k)</pre>
        /// must be true. The current sizes of the two bit sets are not compared.
        /// </summary>
        /// <param name="obj"> the object to compare with </param>
        /// <returns> {@code true} if the objects are the same;
        ///         {@code false} otherwise </returns>
        /// <seealso cref=".size()"/>
        public override bool Equals(object obj)
        {
            if (!(obj is BitSetRecyclable))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }

            BitSetRecyclable set = (BitSetRecyclable)obj;

            checkInvariants();
            set.checkInvariants();

            if (wordsInUse != set.wordsInUse)
            {
                return false;
            }

            // Check words in use by both BitSets
            for (int i = 0; i < wordsInUse; i++)
            {
                if (words[i] != set.words[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Cloning this {@code BitSet} produces a new {@code BitSet}
        /// that is equal to it.
        /// The clone of the bit set is another bit set that has exactly the
        /// same bits set to {@code true} as this bit set.
        /// </summary>
        /// <returns> a clone of this bit set </returns>
        /// <seealso cref=".size()"/>
        public virtual object clone()
        {
            if (!sizeIsSticky)
            {
                trimToSize();
            }

            try
            {
                BitSetRecyclable result = (BitSetRecyclable)base.clone();
                result.words = (long[])words.Clone();
                result.checkInvariants();
                return result;
            }
            catch (CloneNotSupportedException e)
            {
                throw new InternalError(e);
            }
        }

        /// <summary>
        /// Attempts to reduce internal storage used for the bits in this bit set.
        /// Calling this method may, but is not required to, affect the value
        /// returned by a subsequent call to the <seealso cref="size()"/> method.
        /// </summary>
        private void trimToSize()
        {
            if (wordsInUse != words.Length)
            {
                words = Arrays.CopyOf(words, wordsInUse);
                checkInvariants();
            }
        }

        /// <summary>
        /// Save the state of the {@code BitSet} instance to a stream (i.e.,
        /// serialize it).
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private void writeObject(ObjectOutputStream s) throws IOException
        private void writeObject(ObjectOutputStream s)
        {

            checkInvariants();

            if (!sizeIsSticky)
            {
                trimToSize();
            }

            ObjectOutputStream.PutField fields = s.putFields();
            fields.put("bits", words);
            s.writeFields();
        }

        /// <summary>
        /// Reconstitute the {@code BitSet} instance from a stream (i.e.,
        /// deserialize it).
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private void readObject(ObjectInputStream s) throws IOException, ClassNotFoundException
        private void readObject(ObjectInputStream s)
        {

            ObjectInputStream.GetField fields = s.readFields();
            words = (long[])fields.get("bits", null);

            // Assume maximum length then find real length
            // because recalculateWordsInUse assumes maintenance
            // or reduction in logical size
            wordsInUse = words.Length;
            recalculateWordsInUse();
            sizeIsSticky = (words.Length > 0 && words[words.Length - 1] == 0L); // heuristic
            checkInvariants();
        }

        /// <summary>
        /// Returns a string representation of this bit set. For every index
        /// for which this {@code BitSet} contains a bit in the set
        /// state, the decimal representation of that index is included in
        /// the result. Such indices are listed in order from lowest to
        /// highest, separated by ",&nbsp;" (a comma and a space) and
        /// surrounded by braces, resulting in the usual mathematical
        /// notation for a set of integers.
        /// 
        /// <para>Example:
        /// <pre>
        /// BitSet drPepper = new BitSet();</pre>
        /// Now {@code drPepper.toString()} returns "{@code {}}".
        /// <pre>
        /// drPepper.set(2);</pre>
        /// Now {@code drPepper.toString()} returns "{@code {2}}".
        /// <pre>
        /// drPepper.set(4);
        /// drPepper.set(10);</pre>
        /// Now {@code drPepper.toString()} returns "{@code {2, 4, 10}}".
        /// 
        /// </para>
        /// </summary>
        /// <returns> a string representation of this bit set </returns>
        public override string ToString()
        {
            checkInvariants();

            int numBits = (wordsInUse > 128) ? cardinality() : wordsInUse * BITS_PER_WORD;
            StringBuilder b = new StringBuilder(6 * numBits + 2);
            b.Append('{');

            int i = nextSetBit(0);
            if (i != -1)
            {
                b.Append(i);
                while (true)
                {
                    if (++i < 0)
                    {
                        break;
                    }
                    if ((i = nextSetBit(i)) < 0)
                    {
                        break;
                    }
                    int endOfRun = nextClearBit(i);
                    do
                    {
                        b.Append(", ").Append(i);
                    } while (++i != endOfRun);
                }
            }

            b.Append('}');
            return b.ToString();
        }

        public virtual BitSetRecyclable resetWords(long[] words)
        {
            int n;
            for (n = words.Length; n > 0 && words[n - 1] == 0; n--)
            {
                ;
            }
            long[] longs = Arrays.CopyOf(words, n);
            this.words = longs;
            this.wordsInUse = longs.Length;
            checkInvariants();
            return this;
        }

        private Recycler.Handle<BitSetRecyclable> recyclerHandle = null;

        private static readonly Recycler<BitSetRecyclable> RECYCLER = new RecyclerAnonymousInnerClass();

        private class RecyclerAnonymousInnerClass : Recycler<BitSetRecyclable>
        {
            protected internal BitSetRecyclable newObject(Recycler.Handle<BitSetRecyclable> recyclerHandle)
            {
                return new BitSetRecyclable(recyclerHandle);
            }
        }

        private BitSetRecyclable(Recycler.Handle<BitSetRecyclable> recyclerHandle) : this()
        {
            this.recyclerHandle = recyclerHandle;
        }

        public static BitSetRecyclable create()
        {
            return RECYCLER.get();
        }

        public virtual void recycle()
        {
            if (recyclerHandle != null)
            {
                this.clear();
                recyclerHandle.recycle(this);
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}

//Helper class added by Java to C# Converter:

//-------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2023 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replicate the java.nio.ByteBuffer class in C#.
//
//	Instances are only obtainable via the static 'allocate' method.
//
//	Some methods are not available:
//		All methods which create shared views of the buffer such as: array,
//		asCharBuffer, asDoubleBuffer, asFloatBuffer, asIntBuffer, asLongBuffer,
//		asReadOnlyBuffer, asShortBuffer, duplicate, slice, & wrap.
//
//		Other methods such as: mark, reset, isReadOnly, order, compareTo,
//		arrayOffset, & the limit setter method.
//-------------------------------------------------------------------------------------------
using System.IO;

public class ByteBuffer
{
    //'Mode' is only used to determine whether to return data length or capacity from the 'limit' method:
    private enum Mode
    {
        Read,
        Write
    }
    private Mode mode;

    private MemoryStream stream;
    private BinaryReader reader;
    private BinaryWriter writer;

    private ByteBuffer()
    {
        stream = new MemoryStream();
        reader = new BinaryReader(stream);
        writer = new BinaryWriter(stream);
    }

    ~ByteBuffer()
    {
        reader.Close();
        writer.Close();
        stream.Close();
        stream.Dispose();
    }

    public static ByteBuffer allocate(int capacity)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.stream.Capacity = capacity;
        buffer.mode = Mode.Write;
        return buffer;
    }

    public static ByteBuffer allocateDirect(int capacity)
    {
        //this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
        return allocate(capacity);
    }

    public int capacity()
    {
        return stream.Capacity;
    }

    public ByteBuffer flip()
    {
        mode = Mode.Read;
        stream.SetLength(stream.Position);
        stream.Position = 0;
        return this;
    }

    public ByteBuffer clear()
    {
        mode = Mode.Write;
        stream.Position = 0;
        return this;
    }

    public ByteBuffer compact()
    {
        mode = Mode.Write;
        MemoryStream newStream = new MemoryStream(stream.Capacity);
        stream.CopyTo(newStream);
        stream = newStream;
        return this;
    }

    public ByteBuffer rewind()
    {
        stream.Position = 0;
        return this;
    }

    public long limit()
    {
        if (mode == Mode.Write)
            return stream.Capacity;
        else
            return stream.Length;
    }

    public long position()
    {
        return stream.Position;
    }

    public ByteBuffer position(long newPosition)
    {
        stream.Position = newPosition;
        return this;
    }

    public long remaining()
    {
        return this.limit() - this.position();
    }

    public bool hasRemaining()
    {
        return this.remaining() > 0;
    }

    public int get()
    {
        return stream.ReadByte();
    }

    public ByteBuffer get(byte[] dst, int offset, int length)
    {
        stream.Read(dst, offset, length);
        return this;
    }

    public ByteBuffer put(byte b)
    {
        stream.WriteByte(b);
        return this;
    }

    public ByteBuffer put(byte[] src, int offset, int length)
    {
        stream.Write(src, offset, length);
        return this;
    }

    public bool Equals(ByteBuffer other)
    {
        if (other != null && this.remaining() == other.remaining())
        {
            long thisOriginalPosition = this.position();
            long otherOriginalPosition = other.position();

            bool differenceFound = false;
            while (stream.Position < stream.Length)
            {
                if (this.get() != other.get())
                {
                    differenceFound = true;
                    break;
                }
            }

            this.position(thisOriginalPosition);
            other.position(otherOriginalPosition);

            return !differenceFound;
        }
        else
            return false;
    }

    //methods using the internal BinaryReader:
    public char getChar()
    {
        return reader.ReadChar();
    }
    public char getChar(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        char value = reader.ReadChar();
        stream.Position = originalPosition;
        return value;
    }
    public double getDouble()
    {
        return reader.ReadDouble();
    }
    public double getDouble(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        double value = reader.ReadDouble();
        stream.Position = originalPosition;
        return value;
    }
    public float getFloat()
    {
        return reader.ReadSingle();
    }
    public float getFloat(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        float value = reader.ReadSingle();
        stream.Position = originalPosition;
        return value;
    }
    public int getInt()
    {
        return reader.ReadInt32();
    }
    public int getInt(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        int value = reader.ReadInt32();
        stream.Position = originalPosition;
        return value;
    }
    public long getLong()
    {
        return reader.ReadInt64();
    }
    public long getLong(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        long value = reader.ReadInt64();
        stream.Position = originalPosition;
        return value;
    }
    public short getShort()
    {
        return reader.ReadInt16();
    }
    public short getShort(int index)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        short value = reader.ReadInt16();
        stream.Position = originalPosition;
        return value;
    }

    //methods using the internal BinaryWriter:
    public ByteBuffer putChar(char value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putChar(int index, char value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
    public ByteBuffer putDouble(double value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putDouble(int index, double value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
    public ByteBuffer putFloat(float value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putFloat(int index, float value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
    public ByteBuffer putInt(int value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putInt(int index, int value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
    public ByteBuffer putLong(long value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putLong(int index, long value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
    public ByteBuffer putShort(short value)
    {
        writer.Write(value);
        return this;
    }
    public ByteBuffer putShort(int index, short value)
    {
        long originalPosition = stream.Position;
        stream.Position = index;
        writer.Write(value);
        stream.Position = originalPosition;
        return this;
    }
}

//Helper class added by Java to C# Converter:

//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2023 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace some calls to java.util.Arrays methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------
using System;

internal static class Arrays
{
    public static T[] CopyOf<T>(T[] original, int newLength)
    {
        T[] dest = new T[newLength];
        Array.Copy(original, dest, Math.Min(original.Length, newLength));
        return dest;
    }

    public static T[] CopyOfRange<T>(T[] original, int fromIndex, int toIndex)
    {
        int length = toIndex - fromIndex;
        T[] dest = new T[length];
        Array.Copy(original, fromIndex, dest, 0, length);
        return dest;
    }

    public static void Fill<T>(T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
    {
        for (int i = fromIndex; i < toIndex; i++)
        {
            array[i] = value;
        }
    }
}
