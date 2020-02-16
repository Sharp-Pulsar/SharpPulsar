/// <summary>
///*****************************************************************************
/// Copyright 2014 Trevor Robinson
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// *****************************************************************************
/// </summary>

using DotNetty.Buffers;

namespace SharpPulsar.Protocol.Circe
{

	/// <summary>
	/// Represents a stateful hash function, which can accumulate input from multiple
	/// method calls and provide output in various forms. Stateful hash functions
	/// should not be used concurrently from multiple threads.
	/// <para>
	/// Stateful hash functions can be incremental or non-incremental. Incremental
	/// hashing allows calling the <seealso cref="update"/> methods to continue hashing using
	/// accumulated state after calling any of the output methods (such as
	/// <seealso cref="getBytes"/>). Non-incremental hash functions <i>require</i> calling
	/// <seealso cref="reset"/> to reinitialize the state between calling an output method and
	/// an update method.
	/// </para>
	/// </summary>
	public interface StatefulHash : Hash
	{

		/// <summary>
		/// Returns a new instance of this stateful hash function reset to the
		/// initial state.
		/// </summary>
		/// <returns> a new instance of this hash in the initial state </returns>
		StatefulHash CreateNew();

		/// <summary>
		/// Returns whether this hash function supports incremental hashing.
		/// Incremental hashing allows calling the <seealso cref="update"/> methods to
		/// continue hashing using accumulated state after calling any of the output
		/// methods (such as <seealso cref="getBytes"/>). Non-incremental hash functions
		/// require calling <seealso cref="reset"/> to reinitialize the state between calling
		/// an output method and an update method.
		/// </summary>
		/// <returns> true if incremental hashing is supported, false if not </returns>
		bool SupportsIncremental();

		/// <summary>
		/// Resets this hash function to its initial state. Resetting the state is
		/// required for non-incremental hash functions after any output methods have
		/// been called.
		/// </summary>
		void Reset();

		/// <summary>
		/// Updates the state of this hash function with the entire given input
		/// array.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <exception cref="IllegalStateException"> if this hash function is not incremental
		///             but an output method has been called without an intervening
		///             call to <seealso cref="reset"/> </exception>
		void Update(sbyte[] Input);

		/// <summary>
		/// Updates the state of this hash function with the given range of the given
		/// input array.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		/// <exception cref="IllegalArgumentException"> if length is negative </exception>
		/// <exception cref="IndexOutOfBoundsException"> if index is negative or
		///             {@code index + length} is greater than the array length </exception>
		/// <exception cref="IllegalStateException"> if this hash function is not incremental
		///             but an output method has been called without an intervening
		///             call to <seealso cref="reset"/> </exception>
		void Update(sbyte[] Input, int Index, int Length);

		/// <summary>
		/// Updates the state of this hash function with the remaining contents of
		/// the given input buffer. This method leaves the buffer position at the
		/// limit.
		/// </summary>
		/// <param name="input"> the input buffer </param>
		/// <exception cref="IllegalStateException"> if this hash function is not incremental
		///             but an output method has been called without an intervening
		///             call to <seealso cref="reset"/> </exception>
		void Update(IByteBuffer Input);

		/// <summary>
		/// Updates the state of this hash function with memory with the given
		/// address and length. The arguments are generally not checked in any way
		/// and will likely lead to a VM crash or undefined results if invalid.
		/// </summary>
		/// <param name="address"> the base address of the input </param>
		/// <param name="length"> the length of the input </param>
		/// <exception cref="UnsupportedOperationException"> if this function does not support
		///             unsafe memory access </exception>
		/// <exception cref="IllegalStateException"> if this hash function is not incremental
		///             but an output method has been called without an intervening
		///             call to <seealso cref="reset"/> </exception>
		/// <seealso cref= #supportsUnsafe() </seealso>
		void Update(long Address, long Length);

		/// <summary>
		/// Returns a new byte array containing the output of this hash function. The
		/// caller may freely modify the contents of the array.
		/// </summary>
		/// <returns> a new byte array containing the hash output </returns>
		sbyte[] Bytes {get;}

		/// <summary>
		/// Writes the output of this hash function into the given byte array at the
		/// given offset.
		/// </summary>
		/// <param name="output"> the destination array for the output </param>
		/// <param name="index"> the starting index of the first output byte </param>
		/// <param name="maxLength"> the maximum number of bytes to write </param>
		/// <returns> the number of bytes written </returns>
		/// <exception cref="IllegalArgumentException"> if {@code maxLength} is negative </exception>
		/// <exception cref="IndexOutOfBoundsException"> if {@code index} is negative or if
		///             {@code index + maxLength} is greater than the array length </exception>
		int GetBytes(sbyte[] Output, int Index, int MaxLength);

		/// <summary>
		/// Returns the first byte of the output of this hash function.
		/// </summary>
		/// <returns> the first output byte </returns>
		sbyte Byte {get;}

		/// <summary>
		/// Returns the first two bytes of the output of this hash function as a
		/// little-endian {@code short}. If the output is less than two bytes, the
		/// remaining bytes are set to 0.
		/// </summary>
		/// <returns> the first two output bytes as a short </returns>
		short Short {get;}

		/// <summary>
		/// Returns the first four bytes of the output of this hash function as a
		/// little-endian {@code int}. If the output is less than four bytes, the
		/// remaining bytes are set to 0.
		/// </summary>
		/// <returns> the first four output bytes as an int </returns>
		int Int {get;}

		/// <summary>
		/// Returns the first eight bytes of the output of this hash function as a
		/// little-endian {@code long}. If the output is less than eight bytes, the
		/// remaining bytes are set to 0.
		/// </summary>
		/// <returns> the first eight output bytes as a long </returns>
		long Long {get;}
	}

}