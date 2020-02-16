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
	/// Wraps <seealso cref="CRC32"/> in a <seealso cref="StatefulIntHash"/>.
	/// </summary>
	public sealed class JavaCrc32 : AbstractStatefulHash, StatefulIntHash
	{

		private static readonly string _algo = CrcParameters.Crc32.Algorithm();
		private const int _length = 4;

		private readonly Crc32 _impl = new Crc32();

		public override string Algorithm()
		{
			return _algo;
		}

		public override int Length()
		{
			return _length;
		}

		public override StatefulHash CreateNew()
		{
			return new JavaCrc32();
		}

		public override bool SupportsIncremental()
		{
			return true;
		}

		public override void Reset()
		{
			_impl.Reset();
		}

		public override void UpdateUnchecked(sbyte[] input, int index, int length)
		{
			_impl.Update(input, index, length);
		}

		public override int Int => (int) _impl.Value;

        public override long Long => _impl.Value;

        public  StatelessIntHash AsStateless()
		{
			return new AbstractStatelessIntHashAnonymousInnerClass(this);
		}

		public class AbstractStatelessIntHashAnonymousInnerClass : AbstractStatelessIntHash
		{
			private readonly JavaCrc32 _outerInstance;

			public AbstractStatelessIntHashAnonymousInnerClass(JavaCrc32 outerInstance)
			{
				_outerInstance = outerInstance;
			}

			public override string Algorithm()
			{
				return _algo;
			}

			public override int Length()
			{
				return _length;
			}

			public override StatefulIntHash CreateStateful()
			{
				return new JavaCrc32();
			}

			public override int CalculateUnchecked(sbyte[] input, int index, int length)
			{
				Crc32 crc32 = new Crc32();
				crc32.Update(input, index, length);
				return (int) crc32.Value;
			}
		}
	}

}