using System;
using DotNetty.Buffers;

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
namespace SharpPulsar.Protocol.Circe
{

	/// <summary>
	/// Base implementation for stateful hash functions.
	/// </summary>
	public abstract class AbstractStatefulHash : StatefulHash
	{
		public abstract int Length();
		public abstract string Algorithm();
		public abstract long Long {get;}
		public abstract int Int {get;}
		public abstract void Reset();
		public abstract bool SupportsIncremental();
		public abstract StatefulHash CreateNew();

		public bool SupportsUnsafe()
		{
			return false;
		}

		public  void Update(sbyte[] input)
		{
			UpdateUnchecked(input, 0, input.Length);
		}

		public void Update(sbyte[] input, int index, int length)
		{
			if (length < 0)
			{
				throw new ArgumentException();
			}
			if (index < 0 || index + length > input.Length)
			{
				throw new IndexOutOfRangeException();
			}
			UpdateUnchecked(input, index, length);
		}

		public void Update(IByteBuffer input)
		{
			sbyte[] array;
			int index;
			int length = input.ReadableBytes;
			if (input.HasArray)
			{
				array = (sbyte[])(object)input.Array;
				index = input.ArrayOffset + input.ReaderIndex;
				input.SetReaderIndex(input.Capacity);
			}
			else
			{
				// convert to unsafe access if possible
				/*if (input.Direct && SupportsUnsafe())
				{
					long address = UnpooledUnsafeDirectByteBuffer. DirectByteBufferAccessLoader.getAddress(input);
					if (address != 0)
					{
						address += input.position();
						input.position(input.limit());
						Update(address, length);
						return;
					}
				}
				*/
				array = new sbyte[length];
				index = 0;
				//input.GetBytes(array);
			}
			UpdateUnchecked(array, index, length);
		}

		public void Update(long address, long length)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the state of this hash function with the given range of the given
		/// input array. The index and length parameters have already been validated.
		/// </summary>
		/// <param name="input"> the input array </param>
		/// <param name="index"> the starting index of the first input byte </param>
		/// <param name="length"> the length of the input range </param>
		public abstract void UpdateUnchecked(sbyte[] input, int index, int length);

		public virtual sbyte[] Bytes
		{
			get
			{
				sbyte[] array = new sbyte[Length()];
				WriteBytes(array, 0, array.Length);
				return array;
			}
		}

		public virtual int GetBytes(sbyte[] output, int index, int maxLength)
		{
			if (maxLength < 0)
			{
				throw new ArgumentException();
			}
			if (index < 0 || index + maxLength > output.Length)
			{
				throw new IndexOutOfRangeException();
			}
			int length = Math.Min(maxLength, Length());
			WriteBytes(output, index, length);
			return length;
		}

		/// <summary>
		/// Writes the output of this hash function into the given range of the given
		/// byte array. The inputs have already been validated.
		/// </summary>
		/// <param name="output"> the destination array for the output </param>
		/// <param name="index"> the starting index of the first output byte </param>
		/// <param name="length"> the number of bytes to write </param>
		public virtual void WriteBytes(sbyte[] output, int index, int length)
		{
			long temp = Long;
			for (int i = 0; i < length; ++i)
			{
				output[index + i] = (sbyte) temp;
				temp = (long)((ulong)temp >> 8);
			}
		}

		public virtual sbyte Byte
		{
			get
			{
				return (sbyte) Int;
			}
		}

		public virtual short Short
		{
			get
			{
				return (short) Int;
			}
		}
	}

}