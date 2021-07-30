using System;
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
namespace SharpPulsar.Batch
{
	/// <summary>
	/// This this copy of <seealso cref="System.Collections.BitArray"/>.
	/// Provides <seealso cref="BitSet.resetWords(long[])"/> method and leverage with netty recycler.
	/// </summary>
	[Serializable]
	public class BitSet : ICloneable
	{
		/*
		 * BitSets are packed into arrays of "words."  Currently a word is
		 * a long, which consists of 64 bits, requiring 6 address bits.
		 * The choice of word Size is determined purely by performance concerns.
		 */
		private const int AddressBitsPerWord = 6;
		private static readonly int BitsPerWord = 1 << AddressBitsPerWord;
		private static readonly int BitIndexMask = BitsPerWord - 1;

		/* Used to shift left or right for a partial word mask */
		private const long WordMask = unchecked((long)0xffffffffffffffffL);


		/// <summary>
		/// The internal field corresponding to the serialField "bits".
		/// </summary>
		private long[] _words;

		/// <summary>
		/// The number of words in the logical Size of this BitSet.
		/// </summary>
		[NonSerialized]
		private int _wordsInUse = 0;

		/// <summary>
		/// Whether the Size of "words" is user-specified.  If so, we assume
		/// the user knows what he's doing and try harder to preserve it.
		/// </summary>
		[NonSerialized]
		private bool _sizeIsSticky = false;

		/* use serialVersionUID from JDK 1.0.2 for interoperability */
		private const long SerialVersionUid = 7997698588986878753L;

		/// <summary>
		/// Given a bit index, return word index containing it.
		/// </summary>
		private static int WordIndex(int bitIndex)
		{
			return bitIndex >> AddressBitsPerWord;
		}

		/// <summary>
		/// Every public method must preserve these invariants.
		/// </summary>
		private void CheckInvariants()
		{
			Precondition.Condition.Check(_wordsInUse == 0 || _words[_wordsInUse - 1] != 0, string.Empty);
			Precondition.Condition.Check(_wordsInUse >= 0 && _wordsInUse <= _words.Length, string.Empty);
			Precondition.Condition.Check(_wordsInUse == _words.Length || _words[_wordsInUse] == 0, string.Empty);
		}

		/// <summary>
		/// Sets the field wordsInUse to the logical Size in words of the bit set.
		/// WARNING:This method assumes that the number of words actually in use is
		/// less than or equal to the current value of wordsInUse!
		/// </summary>
		private void RecalculateWordsInUse()
		{
			// Traverse the bitset until a used word is found
			int i;
			for (i = _wordsInUse - 1; i >= 0; i--)
			{
				if (_words[i] != 0)
				{
					break;
				}
			}

			_wordsInUse = i + 1; // The new logical Size
		}

		/// <summary>
		/// Creates a new bit set. All bits are initially {@code false}.
		/// </summary>
		public BitSet()
		{
			InitWords(BitsPerWord);
			_sizeIsSticky = false;
		}

		/// <summary>
		/// Creates a bit set whose initial Size is large enough to explicitly
		/// represent bits with indices in the range {@code 0} through
		/// {@code nbits-1}. All bits are initially {@code false}.
		/// </summary>
		/// <param name="nbits"> the initial Size of the bit set </param>
		/// <exception cref="NegativeArraySizeException"> if the specified initial Size
		///         is negative </exception>
		public BitSet(int nbits)
		{
			// nbits can't be negative; Size 0 is OK
			if (nbits < 0)
			{
				throw new Exception("NegativeArraySizeException - nbits < 0: " + nbits);
			}

			InitWords(nbits);
			_sizeIsSticky = true;
		}

		private void InitWords(int nbits)
		{
			_words = new long[WordIndex(nbits - 1) + 1];
		}

		/// <summary>
		/// Creates a bit set using words as the internal representation.
		/// The last word (if there is one) must be non-zero.
		/// </summary>
		private BitSet(long[] words)
		{
			_words = words;
			_wordsInUse = words.Length;
			CheckInvariants();
		}

		/// <summary>
		/// Returns a new bit set containing all the bits in the given long array.
		/// 
		/// <para>More precisely,
		/// <br>{@code BitSet.valueOf(longs).get(n) == ((longs[n/64] & (1L<<(n%64))) != 0)}
		/// <br>for all {@code n < 64 * longs.Length}.
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
		public static BitSet ValueOf(long[] longs)
		{
			int n;
			for (n = longs.Length; n > 0 && longs[n - 1] == 0; n--)
			{
				;
			}
			var lon = new long[n];
			Array.Copy(longs, lon, n);
			return new BitSet(lon);
		}

		/// <summary>
		/// Returns a new long array containing all the bits in this bit set.
		/// 
		/// <para>More precisely, if
		/// <br>{@code long[] longs = s.toLongArray();}
		/// <br>then {@code longs.Length == (s.Length()+63)/64} and
		/// <br>{@code s.get(n) == ((longs[n/64] & (1L<<(n%64))) != 0)}
		/// <br>for all {@code n < 64 * longs.Length}.
		/// 
		/// </para>
		/// </summary>
		/// <returns> a long array containing a little-endian representation
		///         of all the bits in this bit set
		/// @since 1.7 </returns>
		public virtual long[] ToLongArray()
		{
			var words = new long[_wordsInUse];
			Array.Copy(_words, words, _wordsInUse);
			return words;
		}

		/// <summary>
		/// Ensures that the BitSet can hold enough words. </summary>
		/// <param name="wordsRequired"> the minimum acceptable number of words. </param>
		private void EnsureCapacity(int wordsRequired)
		{
			if (_words.Length < wordsRequired)
			{
				// Allocate larger of doubled Size or required Size
				var request = Math.Max(2 * _words.Length, wordsRequired);
				var words = new long[request];
				Array.Copy(_words, words, request);
				_words = words;
				_sizeIsSticky = false;
			}
		}

		/// <summary>
		/// Ensures that the BitSet can accommodate a given wordIndex,
		/// temporarily violating the invariants.  The caller must
		/// restore the invariants before returning to the user,
		/// possibly using recalculateWordsInUse(). </summary>
		/// <param name="wordIndex"> the index to be accommodated. </param>
		private void ExpandTo(int wordIndex)
		{
			var wordsRequired = wordIndex + 1;
			if (_wordsInUse < wordsRequired)
			{
				EnsureCapacity(wordsRequired);
				_wordsInUse = wordsRequired;
			}
		}

		/// <summary>
		/// Checks that fromIndex ... toIndex is a valid range of bit indices.
		/// </summary>
		private static void CheckRange(int fromIndex, int toIndex)
		{
			if (fromIndex < 0)
			{
				throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
			}
			if (toIndex < 0)
			{
				throw new IndexOutOfRangeException("toIndex < 0: " + toIndex);
			}
			if (fromIndex > toIndex)
			{
				throw new IndexOutOfRangeException("fromIndex: " + fromIndex + " > toIndex: " + toIndex);
			}
		}

		/// <summary>
		/// Sets the bit at the specified index to the complement of its
		/// current value.
		/// </summary>
		/// <param name="bitIndex"> the index of the bit to flip </param>
		/// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
		/// @since  1.4 </exception>
		public virtual void Flip(int bitIndex)
		{
			if (bitIndex < 0)
			{
				throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
			}

			var wordIndex = WordIndex(bitIndex);
			ExpandTo(wordIndex);

			_words[wordIndex] ^= (1L << bitIndex);

			RecalculateWordsInUse();
			CheckInvariants();
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
		public virtual void Flip(int fromIndex, int toIndex)
		{
			CheckRange(fromIndex, toIndex);

			if (fromIndex == toIndex)
			{
				return;
			}

			var startWordIndex = WordIndex(fromIndex);
			var endWordIndex = WordIndex(toIndex - 1);
			ExpandTo(endWordIndex);

			var firstWordMask = WordMask << fromIndex;
			var lastWordMask = (WordMask >> -toIndex);
			if (startWordIndex == endWordIndex)
			{
				// Case 1: One word
				_words[startWordIndex] ^= (firstWordMask & lastWordMask);
			}
			else
			{
				// Case 2: Multiple words
				// Handle first word
				_words[startWordIndex] ^= firstWordMask;

				// Handle intermediate words, if any
				for (var i = startWordIndex + 1; i < endWordIndex; i++)
				{
					_words[i] ^= WordMask;
				}

				// Handle last word
				_words[endWordIndex] ^= lastWordMask;
			}

			RecalculateWordsInUse();
			CheckInvariants();
		}

		/// <summary>
		/// Sets the bit at the specified index to {@code true}.
		/// </summary>
		/// <param name="bitIndex"> a bit index </param>
		/// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
		/// @since  JDK1.0 </exception>
		public virtual void Set(int bitIndex)
		{
			if (bitIndex < 0)
			{
				throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
			}

			var wordIndex = WordIndex(bitIndex);
			ExpandTo(wordIndex);

			_words[wordIndex] |= (1L << bitIndex); // Restores invariants

			CheckInvariants();
		}

		/// <summary>
		/// Sets the bit at the specified index to the specified value.
		/// </summary>
		/// <param name="bitIndex"> a bit index </param>
		/// <param name="value"> a boolean value to set </param>
		/// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
		/// @since  1.4 </exception>
		public virtual void Set(int bitIndex, bool value)
		{
			if (value)
			{
				Set(bitIndex);
			}
			else
			{
				Clear(bitIndex);
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
		public virtual void Set(int fromIndex, int toIndex)
		{
			CheckRange(fromIndex, toIndex);

			if (fromIndex == toIndex)
			{
				return;
			}

			// Increase capacity if necessary
			var startWordIndex = WordIndex(fromIndex);
			var endWordIndex = WordIndex(toIndex - 1);
			ExpandTo(endWordIndex);

			var firstWordMask = WordMask << fromIndex;
			var lastWordMask = WordMask >> -toIndex;
			if (startWordIndex == endWordIndex)
			{
				// Case 1: One word
				_words[startWordIndex] |= (firstWordMask & lastWordMask);
			}
			else
			{
				// Case 2: Multiple words
				// Handle first word
				_words[startWordIndex] |= firstWordMask;

				// Handle intermediate words, if any
				for (var i = startWordIndex + 1; i < endWordIndex; i++)
				{
					_words[i] = WordMask;
				}

				// Handle last word (restores invariants)
				_words[endWordIndex] |= lastWordMask;
			}

			CheckInvariants();
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
		public virtual void Set(int fromIndex, int toIndex, bool value)
		{
			if (value)
			{
				Set(fromIndex, toIndex);
			}
			else
			{
				Clear(fromIndex, toIndex);
			}
		}

		/// <summary>
		/// Sets the bit specified by the index to {@code false}.
		/// </summary>
		/// <param name="bitIndex"> the index of the bit to be cleared </param>
		/// <exception cref="IndexOutOfBoundsException"> if the specified index is negative
		/// @since  JDK1.0 </exception>
		public virtual void Clear(int bitIndex)
		{
			if (bitIndex < 0)
			{
				throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
			}

			var wordIndex = WordIndex(bitIndex);
			if (wordIndex >= _wordsInUse)
			{
				return;
			}

			_words[wordIndex] &= ~(1L << bitIndex);

			RecalculateWordsInUse();
			CheckInvariants();
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
		public virtual void Clear(int fromIndex, int toIndex)
		{
			CheckRange(fromIndex, toIndex);

			if (fromIndex == toIndex)
			{
				return;
			}

			var startWordIndex = WordIndex(fromIndex);
			if (startWordIndex >= _wordsInUse)
			{
				return;
			}

			var endWordIndex = WordIndex(toIndex - 1);
			if (endWordIndex >= _wordsInUse)
			{
				toIndex = Length();
				endWordIndex = _wordsInUse - 1;
			}

			var firstWordMask = WordMask << fromIndex;
			var lastWordMask = (WordMask >> -toIndex);
			if (startWordIndex == endWordIndex)
			{
				// Case 1: One word
				_words[startWordIndex] &= ~(firstWordMask & lastWordMask);
			}
			else
			{
				// Case 2: Multiple words
				// Handle first word
				_words[startWordIndex] &= ~firstWordMask;

				// Handle intermediate words, if any
				for (var i = startWordIndex + 1; i < endWordIndex; i++)
				{
					_words[i] = 0;
				}

				// Handle last word
				_words[endWordIndex] &= ~lastWordMask;
			}

			RecalculateWordsInUse();
			CheckInvariants();
		}

		/// <summary>
		/// Sets all of the bits in this BitSet to {@code false}.
		/// 
		/// @since 1.4
		/// </summary>
		public virtual void clear()
		{
			while (_wordsInUse > 0)
			{
				_words[--_wordsInUse] = 0;
			}
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
		public virtual BitSet Get(int fromIndex, int toIndex)
		{
			CheckRange(fromIndex, toIndex);

			CheckInvariants();

			var len = Length();

			// If no set bits in range return empty bitset
			if (len <= fromIndex || fromIndex == toIndex)
			{
				return new BitSet(0);
			}

			// An optimization
			if (toIndex > len)
			{
				toIndex = len;
			}

			var result = new BitSet(toIndex - fromIndex);
			var targetWords = WordIndex(toIndex - fromIndex - 1) + 1;
			var sourceIndex = WordIndex(fromIndex);
			var wordAligned = ((fromIndex & BitIndexMask) == 0);

			// Process all words but the last word
			for (var i = 0; i < targetWords - 1; i++, sourceIndex++)
			{
				result._words[i] = wordAligned ? _words[sourceIndex] : ((long)((ulong)_words[sourceIndex] >> fromIndex)) | (_words[sourceIndex + 1] << -fromIndex);
			}

			// Process the last word
			var lastWordMask = (WordMask >> -toIndex);
			result._words[targetWords - 1] = ((toIndex - 1) & BitIndexMask) < (fromIndex & BitIndexMask) ? (((long)((ulong)_words[sourceIndex] >> fromIndex)) | (_words[sourceIndex + 1] & lastWordMask) << -fromIndex) : ((long)((ulong)(_words[sourceIndex] & lastWordMask) >> fromIndex));

			// Set wordsInUse correctly
			result._wordsInUse = targetWords;
			result.RecalculateWordsInUse();
			result.CheckInvariants();

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
				throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
			}

			CheckInvariants();

			var u = WordIndex(fromIndex);
			if (u >= _wordsInUse)
			{
				return -1;
			}

			var word = _words[u] & (WordMask << fromIndex);

			while (true)
			{
				if (word != 0)
				{
					return (u * BitsPerWord) + NumberOfTrailingZeros(word);
				}
				if (++u == _wordsInUse)
				{
					return -1;
				}
				word = _words[u];
			}
		}
		public static int NumberOfTrailingZeros(long n)
		{
			var mask = 1;
			for (var i = 0; i < 32; i++, mask <<= 1)
				if ((n & mask) != 0)
					return i;

			return 32;
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
			// Neither spec nor implementation handle bitsets of maximal Length.
			// See 4816253.
			if (fromIndex < 0)
			{
				throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
			}

			CheckInvariants();

			var u = WordIndex(fromIndex);
			if (u >= _wordsInUse)
			{
				return fromIndex;
			}

			var word = ~_words[u] & (WordMask << fromIndex);

			while (true)
			{
				if (word != 0)
				{
					return (u * BitsPerWord) + NumberOfTrailingZeros(word);
				}
				if (++u == _wordsInUse)
				{
					return _wordsInUse * BitsPerWord;
				}
				word = ~_words[u];
			}
		}

		/// <summary>
		/// Returns the "logical Size" of this {@code BitSet}: the index of
		/// the highest set bit in the {@code BitSet} plus one. Returns zero
		/// if the {@code BitSet} contains no set bits.
		/// </summary>
		/// <returns> the logical Size of this {@code BitSet}
		/// @since  1.2 </returns>
		public virtual int Length()
		{
			if (_wordsInUse == 0)
			{
				return 0;
			}
			return BitsPerWord * (_wordsInUse - 1) + (BitsPerWord - NumberOfLeadingZeros(_words[_wordsInUse - 1]));
		}
		//https://stackoverflow.com/questions/10439242/count-leading-zeroes-in-an-int32
		private int NumberOfLeadingZeros(long x)
		{
			const int numIntBits = sizeof(int) * 8; //compile time constant
													//do the smearing
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			//count the ones
			x -= x >> 1 & 0x55555555;
			x = (x >> 2 & 0x33333333) + (x & 0x33333333);
			x = (x >> 4) + x & 0x0f0f0f0f;
			x += x >> 8;
			x += x >> 16;
			return (int)(numIntBits - (x & 0x0000003f)); //subtract # of 1s from 32
		}
		private int PopCount(int x)
		{
			x -= ((x >> 1) & 0x55555555);
			x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
			x = (((x >> 4) + x) & 0x0f0f0f0f);
			x += (x >> 8);
			x += (x >> 16);
			return (x & 0x0000003f);
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
				return _wordsInUse == 0;
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
		public virtual bool Intersects(BitSet set)
		{
			for (var i = Math.Min(_wordsInUse, set._wordsInUse) - 1; i >= 0; i--)
			{
				if ((_words[i] & set._words[i]) != 0)
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
		public virtual int Cardinality()
		{
			var sum = 0;
			for (var i = 0; i < _wordsInUse; i++)
			{
				sum += NumberOfSetBits((ulong)_words[i]);
			}
			return sum;
		}
		static int NumberOfSetBits(ulong i)
		{
			i = i - ((i >> 1) & 0x5555555555555555UL);
			i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
			return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
		}
		/// <summary>
		/// Performs a logical <b>AND</b> of this target bit set with the
		/// argument bit set. This bit set is modified so that each bit in it
		/// has the value {@code true} if and only if it both initially
		/// had the value {@code true} and the corresponding bit in the
		/// bit set argument also had the value {@code true}.
		/// </summary>
		/// <param name="set"> a bit set </param>
		public virtual void And(BitSet set)
		{
			if (this == set)
			{
				return;
			}

			while (_wordsInUse > set._wordsInUse)
			{
				_words[--_wordsInUse] = 0;
			}

			// Perform logical AND on words in common
			for (var i = 0; i < _wordsInUse; i++)
			{
				_words[i] &= set._words[i];
			}

			RecalculateWordsInUse();
			CheckInvariants();
		}

		/// <summary>
		/// Performs a logical <b>OR</b> of this bit set with the bit set
		/// argument. This bit set is modified so that a bit in it has the
		/// value {@code true} if and only if it either already had the
		/// value {@code true} or the corresponding bit in the bit set
		/// argument has the value {@code true}.
		/// </summary>
		/// <param name="set"> a bit set </param>
		public virtual void Or(BitSet set)
		{
			if (this == set)
			{
				return;
			}

			var wordsInCommon = Math.Min(_wordsInUse, set._wordsInUse);

			if (_wordsInUse < set._wordsInUse)
			{
				EnsureCapacity(set._wordsInUse);
				_wordsInUse = set._wordsInUse;
			}

			// Perform logical OR on words in common
			for (var i = 0; i < wordsInCommon; i++)
			{
				_words[i] |= set._words[i];
			}

			// Copy any remaining words
			if (wordsInCommon < set._wordsInUse)
			{
				Array.Copy(set._words, wordsInCommon, _words, wordsInCommon, _wordsInUse - wordsInCommon);
			}

			// recalculateWordsInUse() is unnecessary
			CheckInvariants();
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
		public virtual void Xor(BitSet set)
		{
			var wordsInCommon = Math.Min(_wordsInUse, set._wordsInUse);

			if (_wordsInUse < set._wordsInUse)
			{
				EnsureCapacity(set._wordsInUse);
				_wordsInUse = set._wordsInUse;
			}

			// Perform logical XOR on words in common
			for (var i = 0; i < wordsInCommon; i++)
			{
				_words[i] ^= set._words[i];
			}

			// Copy any remaining words
			if (wordsInCommon < set._wordsInUse)
			{
				Array.Copy(set._words, wordsInCommon, _words, wordsInCommon, set._wordsInUse - wordsInCommon);
			}

			RecalculateWordsInUse();
			CheckInvariants();
		}

		/// <summary>
		/// Clears all of the bits in this {@code BitSet} whose corresponding
		/// bit is set in the specified {@code BitSet}.
		/// </summary>
		/// <param name="set"> the {@code BitSet} with which to mask this
		///         {@code BitSet}
		/// @since  1.2 </param>
		public virtual void AndNot(BitSet set)
		{
			// Perform logical (a & !b) on words in common
			for (var i = Math.Min(_wordsInUse, set._wordsInUse) - 1; i >= 0; i--)
			{
				_words[i] &= ~set._words[i];
			}

			RecalculateWordsInUse();
			CheckInvariants();
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
		///     for (int i = words.Length; --i >= 0; )
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
			for (var i = _wordsInUse; --i >= 0;)
			{
				h ^= _words[i] * (i + 1);
			}

			return (int)((h >> 32) ^ h);
		}

		/// <summary>
		/// Returns the number of bits of space actually in use by this
		/// {@code BitSet} to represent bit values.
		/// The maximum element in the set is the Size - 1st element.
		/// </summary>
		/// <returns> the number of bits currently in this bit set </returns>
		public virtual int Size()
		{
			return _words.Length * BitsPerWord;
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
		/// <seealso cref=    #Size() </seealso>
		public override bool Equals(object obj)
		{
			if (!(obj is BitSet))
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}

			var set = (BitSet)obj;

			CheckInvariants();
			set.CheckInvariants();

			if (_wordsInUse != set._wordsInUse)
			{
				return false;
			}

			// Check words in use by both BitSets
			for (var i = 0; i < _wordsInUse; i++)
			{
				if (_words[i] != set._words[i])
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
		/// <seealso cref=    #Size() </seealso>
		public virtual object Clone()
		{
			if (!_sizeIsSticky)
			{
				TrimToSize();
			}

			var result = (BitSet)base.MemberwiseClone();
			result._words = (long[])_words.Clone();
			result.CheckInvariants();
			return result;
		}

		/// <summary>
		/// Attempts to reduce internal storage used for the bits in this bit set.
		/// Calling this method may, but is not required to, affect the value
		/// returned by a subsequent call to the <seealso cref="Size"/> method.
		/// </summary>
		private void TrimToSize()
		{
			if (_wordsInUse != _words.Length)
			{
				Array.Copy(_words, _words, _wordsInUse);
				CheckInvariants();
			}
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
			CheckInvariants();

			var numBits = (_wordsInUse > 128) ? Cardinality() : _wordsInUse * BitsPerWord;
			var b = new StringBuilder(6 * numBits + 2);
			b.Append('{');

			var i = nextSetBit(0);
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
					var endOfRun = nextClearBit(i);
					do
					{
						b.Append(", ").Append(i);
					} while (++i != endOfRun);
				}
			}

			b.Append('}');
			return b.ToString();
		}

		public virtual BitSet ResetWords(long[] words)
		{
			int n;
			for (n = words.Length; n > 0 && words[n - 1] == 0; n--)
			{
				;
			}
			var longs = new long[n];
			Array.Copy(words, longs, n);
			_words = longs;
			_wordsInUse = longs.Length;
			CheckInvariants();
			return this;
		}


		public static BitSet Create()
		{
			return new BitSet();
		}

	}

}