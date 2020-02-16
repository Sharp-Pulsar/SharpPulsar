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
	/// Implements a "reflected" LSB-first int-width CRC function using a lookup
	/// table.
	/// </summary>
	public sealed class ReflectedIntCrc : AbstractIntCrc
	{

		private readonly int[] _table = new int[256];

		public ReflectedIntCrc(string algorithm, int width, int poly, int init, int xorOut) : base(algorithm, width, init, xorOut)
		{

			poly = Reflect(poly);
			for (var i = 0; i < 256; ++i)
			{
				var crc = i;
				for (var j = 0; j < 8; ++j)
				{
					crc = (crc & 1) != 0 ? ((int)((uint)crc >> 1)) ^ poly : (int)((uint)crc >> 1);
				}
				_table[i] = crc;
			}
		}

		public override int Initial()
		{
			return Reflect(base.Initial());
		}

		public override int ResumeRaw(int crc, sbyte[] input, int index, int length)
		{
			for (var i = 0; i < length; ++i)
			{
				crc = _table[(crc ^ input[index + i]) & 0xff] ^ ((int)((uint)crc >> 8));
			}
			return crc;
		}
	}

}