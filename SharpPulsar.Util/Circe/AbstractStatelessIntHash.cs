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
	/// Base implementation for stateless (but not incremental) integer hash
	/// functions.
	/// </summary>
	public abstract class AbstractStatelessIntHash : StatelessIntHash
	{
		public abstract int Length();
		public abstract string Algorithm();
		public abstract StatefulIntHash CreateStateful();

		public bool SupportsUnsafe()
		{
			return false;
		}

		public int Calculate(sbyte[] input)
		{
			return CalculateUnchecked(input, 0, input.Length);
		}

		public int Calculate(sbyte[] input, int index, int length)
		{
			if (length < 0)
			{
				throw new System.ArgumentException();
			}
			if (index < 0 || index + length > input.Length)
			{
				throw new System.IndexOutOfRangeException();
			}
			return CalculateUnchecked(input, index, length);
		}

		public int Calculate(ByteBuffer input)
		{
			sbyte[] array;
			int index;
			int length = input.remaining();
			if (input.hasArray())
			{
				array = input.array();
				index = input.arrayOffset() + input.position();
				input.position(input.limit());
			}
			else
			{
				array = new sbyte[length];
				index = 0;
				input.get(array);
			}
			return CalculateUnchecked(array, index, length);
		}

		public int Calculate(long address, long length)
		{
			throw new System.NotSupportedException();
		}

		/// <summary>
		/// Evaluates this hash function for the given range of the given input
		/// array. The index and length parameters have already been validated.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		/// <returns> the output of the hash function </returns>
		public abstract int CalculateUnchecked(sbyte[] input, int index, int length);
	}

}