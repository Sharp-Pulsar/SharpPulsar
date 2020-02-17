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
	/// Base implementation for incremental stateless integer hash functions.
	/// </summary>
	public abstract class AbstractIncrementalIntHash : IncrementalIntHash
	{
        public string Algorithm()
        {
            throw new System.NotImplementedException();
        }

        public int Length()
        {
            throw new System.NotImplementedException();
        }

        public bool SupportsUnsafe()
		{
			return false;
		}

        StatefulHash StatelessHash.CreateStateful()
        {
            return CreateStateful();
        }

        public StatefulIntHash CreateStateful()
		{
			return new IncrementalIntStatefulHash(this);
		}

		public int Calculate(sbyte[] input)
		{
			return Resume(Initial(), input);
		}

		public int Calculate(sbyte[] input, int index, int length)
		{
			return Resume(Initial(), input, index, length);
		}

		public int Calculate(IByteBuffer input)
		{
			return Resume(Initial(), input);
		}

		public int Calculate(long address, long length)
		{
			return Resume(Initial(), address, length);
		}

		public int Resume(int current, sbyte[] input)
		{
			return ResumeUnchecked(current, input, 0, input.Length);
		}

		public int Resume(int current, sbyte[] input, int index, int length)
		{
			if (length < 0)
			{
				throw new System.ArgumentException();
			}
			if (index < 0 || index + length > input.Length)
			{
				throw new System.IndexOutOfRangeException();
			}
			return ResumeUnchecked(current, input, index, length);
		}

        public int Resume(int Current, ByteBuffer Input)
        {
            throw new System.NotImplementedException();
        }

        public  int Resume(int current, IByteBuffer input)
		{
			sbyte[] array;
			int index;
			var length = input.Array.Length;
			if (input.HasArray)
			{
				array = (sbyte[])(object)input.Array;
				index = input.ArrayOffset + input.ReaderIndex;
				input.SetReaderIndex(input.Capacity);
			}
			else
			{
				array = new sbyte[length];
				var ar = new byte[length];
				index = 0;
				input.GetBytes(index, ar);
                array = (sbyte[]) (object) ar;
            }
			return ResumeUnchecked(current, array, index, length);
		}

		public int Resume(int current, long address, long length)
		{
			throw new System.NotSupportedException();
		}

		/// <summary>
		/// The initial state of the hash function, which is the same as the output
		/// value for an empty input sequence.
		/// </summary>
		/// <returns> the initial hash state/output </returns>
		public abstract int Initial();

		/// <summary>
		/// Evaluates this hash function as if the given range of the given input
		/// array were appended to the previously hashed input. The index and length
		/// parameters have already been validated.
		/// </summary>
		/// <param name="current"> the hash output for input hashed so far </param>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		/// <returns> the output of the hash function for the concatenated input </returns>
		public abstract int ResumeUnchecked(int current, sbyte[] input, int index, int length);
	}

}