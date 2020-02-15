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
	/// Hash parameters used to define a <a
	/// href="http://en.wikipedia.org/wiki/Cyclic_redundancy_check">cyclic redundancy
	/// check</a> (CRC). Includes some commonly used sets of parameters.
	/// </summary>
	public class CrcParameters : HashParameters
	{

		private readonly string _name;
		private readonly int _bitWidth;
		private readonly long _polynomial;
		private readonly long _initial;
		private readonly long _xorOut;
		private readonly bool _reflected;

		/// <summary>
		/// Constructs a <seealso cref="CrcParameters"/> with the given parameters.
		/// </summary>
		/// <param name="name"> the canonical name of the CRC function </param>
		/// <param name="bitWidth"> the width of the CRC function </param>
		/// <param name="polynomial"> the polynomial in binary form (non-reflected) </param>
		/// <param name="initial"> the initial value of the CRC register </param>
		/// <param name="xorOut"> a value XOR'ed with the CRC when it is read </param>
		/// <param name="reflected"> indicates whether the CRC is reflected (LSB-first) </param>
		/// <exception cref="IllegalArgumentException"> if the width is less than 1 or greater
		///             than 64 </exception>
		public CrcParameters(string name, int bitWidth, long polynomial, long initial, long xorOut, bool reflected)
		{
			if (bitWidth < 1 || bitWidth > 64)
			{
				throw new System.ArgumentException();
			}
			_name = name;
			_bitWidth = bitWidth;
			long mask = bitWidth < 64 ? (1L << bitWidth) - 1 :~0L;
			_polynomial = polynomial & mask;
			_initial = initial & mask;
			_xorOut = xorOut & mask;
			_reflected = reflected;
		}

		public string Algorithm()
		{
			return _name;
		}

		/// <summary>
		/// Returns the width in bits of the CRC function. The width is also the
		/// position of the implicit set bit at the top of the polynomial.
		/// </summary>
		/// <returns> the CRC width in bits </returns>
		public virtual int BitWidth()
		{
			return _bitWidth;
		}

		/// <summary>
		/// Returns the binary form of polynomial that defines the CRC function (with
		/// the implicit top bit omitted). For instance, the CRC-16 polynomial
		/// x<sup>16</sup> + x<sup>15</sup> + x<sup>2</sup> + 1 is represented as
		/// {@code 1000 0000 0000 0101} ({@code 0x8005}).
		/// </summary>
		/// <returns> the CRC polynomial </returns>
		public virtual long Polynomial()
		{
			return _polynomial;
		}

		/// <summary>
		/// Returns the initial value of the CRC register.
		/// </summary>
		/// <returns> the CRC initial value </returns>
		public virtual long Initial()
		{
			return _initial;
		}

		/// <summary>
		/// Returns the value XOR'ed with the CRC register when it is read to
		/// determine the output value.
		/// </summary>
		/// <returns> the final XOR value </returns>
		public virtual long XorOut()
		{
			return _xorOut;
		}

		/// <summary>
		/// Returns whether the CRC function is "reflected". Reflected CRCs process
		/// data LSB-first, whereas "normal" CRCs are MSB-first.
		/// </summary>
		/// <returns> whether the CRC function is reflected </returns>
		public virtual bool Reflected()
		{
			return _reflected;
		}

		/// <summary>
		/// Returns whether this object matches the given CRC parameters.
		/// </summary>
		/// <param name="bitWidth"> the width of the CRC function </param>
		/// <param name="polynomial"> the polynomial in binary form (non-reflected) </param>
		/// <param name="initial"> the initial value of the CRC register </param>
		/// <param name="xorOut"> a value XOR'ed with the CRC when it is read </param>
		/// <param name="reflected"> indicates whether the CRC is reflected (LSB-first) </param>
		/// <returns> true if all parameters match exactly, false otherwise </returns>
		public virtual bool Match(int bitWidth, long polynomial, long initial, long xorOut, bool reflected)
		{
			return bitWidth == _bitWidth && polynomial == _polynomial && initial == _initial && xorOut == _xorOut && reflected == _reflected;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			CrcParameters other = (CrcParameters) obj;
			return _bitWidth == other._bitWidth && _polynomial == other._polynomial && _initial == other._initial && _xorOut == other._xorOut && _reflected == other._reflected;
		}

		public override int GetHashCode()
		{
			return (int)(_polynomial ^ ((long)((ulong)_polynomial >> 32)) ^ _initial ^ ((long)((ulong)_initial >> 32)) ^ _xorOut ^ ((long)((ulong)_xorOut >> 32))) ^ (_reflected ?~0 : 0);
		}

		/// <summary>
		/// Parameters for CRC-16, used in the ARC and LHA compression utilities.
		/// </summary>
		public static readonly CrcParameters Crc16 = new CrcParameters("CRC-16", 16, 0x8005, 0, 0, true);

		/// <summary>
		/// Parameters for CRC-16/CCITT, used in the Kermit protocol.
		/// </summary>
		public static readonly CrcParameters Crc16Ccitt = new CrcParameters("CRC-16/CCITT", 16, 0x1021, 0, 0, true);

		/// <summary>
		/// Parameters for CRC-16/XMODEM, used in the XMODEM protocol.
		/// </summary>
		public static readonly CrcParameters Crc16Xmodem = new CrcParameters("CRC-16/XMODEM", 16, 0x1021, 0, 0, false);

		/// <summary>
		/// Parameters for CRC-32, used in Ethernet, SATA, PKZIP, ZMODEM, etc.
		/// </summary>
		public static readonly CrcParameters Crc32 = new CrcParameters("CRC-32", 32, 0x04c11db7, ~0, ~0, true);

		/// <summary>
		/// Parameters for CRC-32/BZIP2, used in BZIP2.
		/// </summary>
		public static readonly CrcParameters Crc32Bzip2 = new CrcParameters("CRC-32/BZIP2", 32, 0x04c11db7, ~0, ~0, false);

		/// <summary>
		/// Parameters for CRC-32C, used in iSCSI and SCTP.
		/// </summary>
		public static readonly CrcParameters Crc32C = new CrcParameters("CRC-32C", 32, 0x1edc6f41, ~0, ~0, true);

		/// <summary>
		/// Parameters for CRC-32/MPEG-2, used in MPEG-2.
		/// </summary>
		public static readonly CrcParameters Crc32Mpeg2 = new CrcParameters("CRC-32/MPEG-2", 32, 0x04c11db7, ~0, 0, false);

		/// <summary>
		/// Parameters for CRC-32/POSIX, used in the {@code cksum} utility.
		/// </summary>
		public static readonly CrcParameters Crc32Posix = new CrcParameters("CRC-32/POSIX", 32, 0x04c11db7, 0, ~0, false);

		/// <summary>
		/// Parameters for CRC-64, used in the ECMA-182 standard for DLT-1 tapes.
		/// </summary>
		public static readonly CrcParameters Crc64 = new CrcParameters("CRC-64", 64, 0x42f0e1eba9ea3693L, 0L, 0L, false);

		/// <summary>
		/// Parameters for CRC-64/XZ, used in the {@code .xz} file format.
		/// </summary>
		public static readonly CrcParameters Crc64Xz = new CrcParameters("CRC-64/XZ", 64, 0x42f0e1eba9ea3693L, ~0L, ~0L, true);
	}

}