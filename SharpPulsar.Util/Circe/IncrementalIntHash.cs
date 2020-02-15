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
namespace SharpPulsar.Util.Circe
{

	/// <summary>
	/// Incremental stateless integer hash function, which has the property that its
	/// output is the same as (or easily derivable from) its state. Specifically, for
	/// any sequence M partitioned arbitrarily into two subsequences M<sub>1</sub>
	/// M<sub>2</sub>:
	/// 
	/// <pre>
	/// h(M) = h'(h(M<sub>1</sub>), M<sub>2</sub>)
	/// </pre>
	/// 
	/// where h corresponds to the <seealso cref="StatelessIntHash.calculate calculate"/>
	/// function and h' corresponds to the <seealso cref="resume"/> function.
	/// <para>
	/// Note that stateful hash functions obtained from incremental stateless hash
	/// functions are also <seealso cref="StatefulHash.supportsIncremental incremental"/>.
	/// </para>
	/// </summary>
	public interface IncrementalIntHash : StatelessIntHash
	{

		/// <summary>
		/// Evaluates this hash function as if the entire given input array were
		/// appended to the previously hashed input.
		/// </summary>
		/// <param name="current"> the hash output for input hashed so far </param>
		/// <param name="input"> the input array </param>
		/// <returns> the output of the hash function for the concatenated input </returns>
		int Resume(int Current, sbyte[] Input);

		/// <summary>
		/// Evaluates this hash function as if the given range of the given input
		/// array were appended to the previously hashed input.
		/// </summary>
		/// <param name="current"> the hash output for input hashed so far </param>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		/// <returns> the output of the hash function for the concatenated input </returns>
		/// <exception cref="IndexOutOfBoundsException"> if index is negative or
		///             {@code index + length} is greater than the array length </exception>
		int Resume(int Current, sbyte[] Input, int Index, int Length);

		/// <summary>
		/// Evaluates this hash function as if the remaining contents of the given
		/// input buffer were appended to the previously hashed input. This method
		/// leaves the buffer position at the limit.
		/// </summary>
		/// <param name="current"> the hash output for input hashed so far </param>
		/// <param name="input"> the input buffer </param>
		/// <returns> the output of the hash function for the concatenated input </returns>
		int Resume(int Current, ByteBuffer Input);

		/// <summary>
		/// Evaluates this hash function as if the memory with the given address and
		/// length were appended to the previously hashed input. The arguments are
		/// generally not checked in any way and will likely lead to a VM crash or
		/// undefined results if invalid.
		/// </summary>
		/// <param name="current"> the hash output for input hashed so far </param>
		/// <param name="address"> the base address of the input </param>
		/// <param name="length"> the length of the input </param>
		/// <returns> the output of the hash function for the concatenated input </returns>
		/// <exception cref="UnsupportedOperationException"> if this function does not support
		///             unsafe memory access </exception>
		/// <seealso cref= #supportsUnsafe() </seealso>
		int Resume(int Current, long Address, long Length);
	}

}