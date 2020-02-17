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
	/// Implements a "normal" MSB-first long-width CRC function using a lookup table.
	/// Does not support bit-widths less than 8.
	/// </summary>
	public sealed class NormalLongCrc : AbstractLongCrc
	{

		private readonly long _widthMask;
		private readonly long[] _table = new long[256];

		public NormalLongCrc(string algorithm, int bitWidth, long poly, long init, long xorOut) : base(algorithm, bitWidth, init, xorOut)
		{
			if (bitWidth < 8)
			{
				throw new System.ArgumentException("invalid CRC width");
			}

			_widthMask = bitWidth < 64 ? ((1L << bitWidth) - 1) :~0L;
			var top = 1L << (bitWidth - 1);
			for (var i = 0; i < 256; ++i)
			{
				var crc = (long) i << (bitWidth - 8);
				for (var j = 0; j < 8; ++j)
				{
					crc = (crc & top) != 0 ? (crc << 1) ^ poly : crc << 1;
				}
				_table[i] = crc & _widthMask;
			}
		}

		public override long ResumeRaw(long crc, sbyte[] input, int index, int length)
		{
			for (var i = 0; i < length; ++i)
			{
				crc = _table[(int)(((long)((ulong)crc >> (BitWidth - 8))) ^ input[index + i]) & 0xff] ^ (crc << 8);
			}
			return crc & _widthMask;
		}
	}

}