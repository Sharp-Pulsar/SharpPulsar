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
	/// Promotes a <seealso cref="StatelessIntHash"/> to a <seealso cref="StatelessLongHash"/>.
	/// </summary>
	public sealed class IntStatelessLongHash : StatelessLongHash
	{

		private readonly StatelessIntHash _intHash;

		/// <summary>
		/// Constructs a new <seealso cref="IntStatelessLongHash"/> that delegates to the given
		/// <seealso cref="StatelessIntHash"/>.
		/// </summary>
		/// <param name="intHash"> the underlying int-width hash </param>
		public IntStatelessLongHash(StatelessIntHash intHash)
		{
			this._intHash = intHash;
		}

		public string Algorithm()
		{
			return _intHash.Algorithm();
		}

		public int Length()
		{
			return _intHash.Length();
		}

		public bool SupportsUnsafe()
		{
			return _intHash.SupportsUnsafe();
		}

        StatefulHash StatelessHash.CreateStateful()
        {
            return CreateStateful();
        }

        public StatefulLongHash CreateStateful()
		{
			return new IntStatefulLongHash(_intHash.CreateStateful());
		}

		public long Calculate(sbyte[] input)
		{
			return _intHash.Calculate(input);
		}

		public long Calculate(sbyte[] input, int index, int length)
		{
			return _intHash.Calculate(input, index, length);
		}

		public  long Calculate(ByteBuffer input)
		{
			return _intHash.Calculate(input);
		}

		public long Calculate(long address, long length)
		{
			return _intHash.Calculate(address, length);
		}
	}

}