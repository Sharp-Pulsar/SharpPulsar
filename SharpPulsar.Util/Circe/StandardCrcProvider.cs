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

using System.Collections.Generic;

namespace SharpPulsar.Util.Circe
{
    using CrcParameters = CrcParameters;

	/// <summary>
	/// Provides pure Java and JDK-supplied CRC implementations.
	/// </summary>
	public sealed class StandardCrcProvider : AbstractHashProvider<Circe.CrcParameters>
	{

		/// <summary>
		/// Constructs a new <seealso cref="StandardCrcProvider"/>.
		/// </summary>
		public StandardCrcProvider() : base(typeof(Circe.CrcParameters))
		{
		}

		public override ISet<HashSupport> QuerySupportTyped(Circe.CrcParameters @params)
		{
			ISet<HashSupport> result = new HashSet<HashSupport>(){ HashSupport.Stateful, HashSupport.Incremental, HashSupport.StatelessIncremental, HashSupport.LongSized };
			if (@params.BitWidth() <= 32)
			{
				result.Add(HashSupport.IntSized);
			}
			if (@params.Equals(Circe.CrcParameters.Crc32))
			{
				result.Add(HashSupport.Native);
			}
			return result;
		}

		public override Hash Get(Circe.CrcParameters @params, ISet<HashSupport> required)
		{
			if (!required.Contains(HashSupport.StatelessIncremental) && @params.Equals(Circe.CrcParameters.Crc32))
			{
				return new JavaCrc32();
			}
			if (required.Contains(HashSupport.Native))
			{
				throw new System.NotSupportedException();
			}
			return GetCacheable(@params, required);
		}

		public override StatelessHash CreateCacheable(Circe.CrcParameters @params, EnumSet<HashSupport> required)
		{
			int bitWidth = @params._bitWidth();
			if (bitWidth > 32 || (required.contains(HashSupport.LONG_SIZED) && bitWidth >= 8))
			{
				if (required.contains(HashSupport.INT_SIZED))
				{
					throw new System.NotSupportedException();
				}
				if (@params._reflected())
				{
					return new ReflectedLongCrc(@params.algorithm(), bitWidth, @params._polynomial(), @params._initial(), @params._xorOut());
				}
				else
				{
					return new NormalLongCrc(@params.algorithm(), bitWidth, @params._polynomial(), @params._initial(), @params._xorOut());
				}
			}
			else
			{
				if (@params._reflected())
				{
					return new ReflectedIntCrc(@params.algorithm(), bitWidth, (int) @params._polynomial(), (int) @params._initial(), (int) @params._xorOut());
				}
				else if (bitWidth > 8)
				{
					return new NormalIntCrc(@params.algorithm(), bitWidth, (int) @params._polynomial(), (int) @params._initial(), (int) @params._xorOut());
				}
				return new NormalByteCrc(@params.algorithm(), bitWidth, (int) @params._polynomial(), (int) @params._initial(), (int) @params._xorOut());
			}
		}
	}

}