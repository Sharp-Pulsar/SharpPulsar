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
	/// Wraps <seealso cref="CRC32"/> in a <seealso cref="StatefulIntHash"/>.
	/// </summary>
	public sealed class JavaCrc32 : AbstractStatefulHash, StatefulIntHash
	{

		private static readonly string ALGORITHM = CrcParameters.Crc32.Algorithm();
		private const int LENGTH = 4;

		private readonly crc32 impl = new CRC32();

		public override string Algorithm()
		{
			return ALGORITHM;
		}

		public override int Length()
		{
			return LENGTH;
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
			impl.reset();
		}

		public override void UpdateUnchecked(sbyte[] input, int index, int length)
		{
			impl.update(input, index, length);
		}

		public override int Int
		{
			get
			{
				return (int) impl.Value;
			}
		}

		public override long Long
		{
			get
			{
				return impl.Value;
			}
		}

		public  StatelessIntHash AsStateless()
		{
			return new AbstractStatelessIntHashAnonymousInnerClass(this);
		}

		public class AbstractStatelessIntHashAnonymousInnerClass : AbstractStatelessIntHash
		{
			private readonly JavaCrc32 outerInstance;

			public AbstractStatelessIntHashAnonymousInnerClass(JavaCrc32 OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public  string Algorithm()
			{
				return ALGORITHM;
			}

			public  int Length()
			{
				return LENGTH;
			}

			public  StatefulIntHash CreateStateful()
			{
				return new JavaCrc32();
			}

			public int CalculateUnchecked(sbyte[] input, int index, int length)
			{
//ORIGINAL LINE: final java.util.zip.CRC32 crc32 = new java.util.zip.CRC32();
				CRC32 crc32 = new CRC32();
				crc32.update(input, index, length);
				return (int) crc32.Value;
			}
		}
	}

}