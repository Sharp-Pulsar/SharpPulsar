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
	/// Base implementation of long-width CRC functions.
	/// </summary>
	public abstract class AbstractLongCrc : AbstractIncrementalLongHash
	{

		private readonly string _algorithm;
		protected internal readonly int BitWidth;
		private readonly long _initial;
		private readonly long _xorOut;

        protected AbstractLongCrc(string algorithm, int bitWidth, long initial, long xorOut)
		{
			if (bitWidth < 1 || bitWidth > 64)
			{
				throw new System.ArgumentException("invalid CRC width");
			}
			_algorithm = algorithm;
			BitWidth = bitWidth;
			_initial = initial;
			_xorOut = xorOut;
		}

		public string Algorithm()
		{
			return _algorithm;
		}

		public int Length()
		{
			return (BitWidth + 7) / 8;
		}

		public override long Initial()
		{
			return _initial ^ _xorOut;
		}

		public override long ResumeUnchecked(long current, sbyte[] input, int index, int length)
		{
			return ResumeRaw(current ^ _xorOut, input, index, length) ^ _xorOut;
		}

		public abstract long ResumeRaw(long crc, sbyte[] input, int index, int length);

		public long Reflect(long value)
		{
			return (int)((uint)Reverser.ReverseBytes(value) >> (64 - BitWidth));
		}
	}

}