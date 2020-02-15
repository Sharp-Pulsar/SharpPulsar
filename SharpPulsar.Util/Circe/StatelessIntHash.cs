﻿/// <summary>
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
	/// Interface implemented by stateless hash functions with an output length of 4
	/// bytes or less.
	/// </summary>
	public interface StatelessIntHash : StatelessHash
	{

		/// <summary>
		/// Returns a new instance of stateful version of this hash function.
		/// </summary>
		/// <returns> the stateful version of this hash function </returns>
        new StatefulIntHash CreateStateful();

		/// <summary>
		/// Evaluates this hash function for the entire given input array.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <returns> the output of the hash function </returns>
		int Calculate(sbyte[] Input);

		/// <summary>
		/// Evaluates this hash function for the given range of the given input
		/// array.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		/// <returns> the output of the hash function </returns>
		/// <exception cref="IndexOutOfBoundsException"> if index is negative or
		///             {@code index + length} is greater than the array length </exception>
		int Calculate(sbyte[] Input, int Index, int Length);

		/// <summary>
		/// Evaluates this hash function with the remaining contents of the given
		/// input buffer. This method leaves the buffer position at the limit.
		/// </summary>
		/// <param name="input"> the input buffer </param>
		/// <returns> the output of the hash function </returns>
		int Calculate(ByteBuffer Input);

		/// <summary>
		/// Evaluates this hash function for the memory with the given address and
		/// length. The arguments are generally not checked in any way and will
		/// likely lead to a VM crash or undefined results if invalid.
		/// </summary>
		/// <param name="address"> the base address of the input </param>
		/// <param name="length"> the length of the input </param>
		/// <returns> the output of the hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this function does not support
		///             unsafe memory access </exception>
		/// <seealso cref= #supportsUnsafe() </seealso>
		int Calculate(long Address, long Length);
	}

}