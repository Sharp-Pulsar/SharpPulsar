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
	/// Implements a "normal" MSB-first byte-width CRC function using a lookup table.
	/// </summary>
	public sealed class NormalByteCrc : AbstractIntCrc
	{

		private readonly sbyte[] _table = new sbyte[256];

		public NormalByteCrc(string algorithm, int bitWidth, int poly, int init, int xorOut) : base(algorithm, bitWidth, init, xorOut)
		{
			if (bitWidth > 8)
			{
				throw new System.ArgumentException("invalid CRC width");
			}

			var widthMask = (1 << bitWidth) - 1;
			var shpoly = poly << (8 - bitWidth);
			for (var i = 0; i < 256; ++i)
			{
				var crc = i;
				for (var j = 0; j < 8; ++j)
				{
					crc = (crc & 0x80) != 0 ? (crc << 1) ^ shpoly : crc << 1;
				}
				_table[i] = (sbyte)((crc >> (8 - bitWidth)) & widthMask);
			}
		}

		public override int ResumeRaw(int crc, sbyte[] input, int index, int length)
		{
			for (var i = 0; i < length; ++i)
			{
				crc = _table[(crc << (8 - BitWidth)) ^ (input[index + i] & 0xff)] & 0xff;
			}
			return crc;
		}
	}

}