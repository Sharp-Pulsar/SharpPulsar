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

namespace SharpPulsar.Protocol.Circe
{
    /// <summary>
	/// Provides pure Java and JDK-supplied CRC implementations.
	/// </summary>
	public sealed class StandardCrcProvider : AbstractHashProvider<CrcParameters>
	{
		/// <summary>
		/// Constructs a new <seealso cref="StandardCrcProvider"/>.
		/// </summary>
		public StandardCrcProvider() : base(typeof(CrcParameters))
		{
		}

		public override ISet<HashSupport> QuerySupportTyped(CrcParameters @params)
		{
			ISet<HashSupport> result = new HashSet<HashSupport>(){ HashSupport.Stateful, HashSupport.Incremental, HashSupport.StatelessIncremental, HashSupport.LongSized };
			if (@params.BitWidth() <= 32)
			{
				result.Add(HashSupport.IntSized);
			}
			if (@params.Equals(CrcParameters.Crc32))
			{
				result.Add(HashSupport.Native);
			}
			return result;
		}

		public override Hash Get(CrcParameters @params, ISet<HashSupport> required)
		{
			if (!required.Contains(HashSupport.StatelessIncremental) && @params.Equals(CrcParameters.Crc32))
			{
				return new JavaCrc32();
			}
			if (required.Contains(HashSupport.Native))
			{
				throw new System.NotSupportedException();
			}
			return GetCacheable(@params, required);
		}

		public override StatelessHash CreateCacheable(CrcParameters @params, ISet<HashSupport> required)
		{
			var bitWidth = @params.BitWidth();
			if (bitWidth > 32 || (required.Contains(HashSupport.LongSized) && bitWidth >= 8))
			{
				if (required.Contains(HashSupport.IntSized))
				{
					throw new System.NotSupportedException();
				}
				if (@params.Reflected())
				{
					return new ReflectedLongCrc(@params.Algorithm(), bitWidth, @params.Polynomial(), @params.Initial(), @params.XorOut());
				}
				else
				{
					return new NormalLongCrc(@params.Algorithm(), bitWidth, @params.Polynomial(), @params.Initial(), @params.XorOut());
				}
			}
			else
			{
				if (@params.Reflected())
				{
					return new ReflectedIntCrc(@params.Algorithm(), bitWidth, (int) @params.Polynomial(), (int) @params.Initial(), (int) @params.XorOut());
				}
				else if (bitWidth > 8)
				{
					return new NormalIntCrc(@params.Algorithm(), bitWidth, (int) @params.Polynomial(), (int) @params.Initial(), (int) @params.XorOut());
				}
				return new NormalByteCrc(@params.Algorithm(), bitWidth, (int) @params.Polynomial(), (int) @params.Initial(), (int) @params.XorOut());
			}
		}
	}

}