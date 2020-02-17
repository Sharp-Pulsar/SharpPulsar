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
	/// Promotes a <seealso cref="StatefulIntHash"/> to a <seealso cref="StatefulLongHash"/>.
	/// </summary>
	public sealed class IntStatefulLongHash : StatefulLongHash
	{

		private readonly StatefulIntHash _intHash;

		/// <summary>
		/// Constructs a new <seealso cref="IntStatefulLongHash"/> that delegates to the given
		/// <seealso cref="StatefulIntHash"/>.
		/// </summary>
		/// <param name="intHash"> the underlying int-width hash </param>
		public IntStatefulLongHash(StatefulIntHash intHash)
		{
			_intHash = intHash;
		}

		public StatelessLongHash AsStateless()
		{
			return new IntStatelessLongHash(_intHash.AsStateless());
		}

		public string Algorithm()
		{
			return _intHash.Algorithm();
		}

		public int Length()
		{
			return _intHash.Length();
		}

		public StatefulHash CreateNew()
		{
			return _intHash.CreateNew();
		}

		public bool SupportsUnsafe()
		{
			return _intHash.SupportsUnsafe();
		}

		public bool SupportsIncremental()
		{
			return _intHash.SupportsIncremental();
		}

		public void Reset()
		{
			_intHash.Reset();
		}

		public void Update(sbyte[] input)
		{
			_intHash.Update(input);
		}

		public void Update(sbyte[] input, int index, int length)
		{
			_intHash.Update(input, index, length);
		}

		public void Update(IByteBuffer input)
		{
			_intHash.Update(input);
		}

		public void Update(long address, long length)
		{
			_intHash.Update(address, length);
		}

		public sbyte[] Bytes => _intHash.Bytes;

        public int GetBytes(sbyte[] output, int index, int maxLength)
		{
			return _intHash.GetBytes(output, index, maxLength);
		}

		public sbyte Byte => _intHash.Byte;

        public short Short => _intHash.Short;

        public int Int => _intHash.Int;

        public long Long => _intHash.Long;
    }

}