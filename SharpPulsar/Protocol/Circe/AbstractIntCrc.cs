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
	/// Base implementation of int-width CRC functions.
	/// </summary>
	public abstract class AbstractIntCrc : AbstractIncrementalIntHash
	{

		private readonly string _algorithm;
		protected internal readonly int BitWidth;
		private readonly int _initial;
		private readonly int _xorOut;

        protected AbstractIntCrc(string algorithm, int bitWidth, int initial, int xorOut)
		{
			if (bitWidth < 1 || bitWidth > 32)
			{
				throw new System.ArgumentException("invalid CRC width");
			}
			_algorithm = algorithm;
			BitWidth = bitWidth;
			_initial = initial;
			_xorOut = xorOut;
		}

		public new string Algorithm()
		{
			return _algorithm;
		}

		public new int Length()
		{
			return (BitWidth + 7) / 8;
		}

		public override int Initial()
		{
			return _initial ^ _xorOut;
		}

		public override int ResumeUnchecked(int current, sbyte[] input, int index, int length)
		{
			return ResumeRaw(current ^ _xorOut, input, index, length) ^ _xorOut;
		}

		public abstract int ResumeRaw(int crc, sbyte[] input, int index, int length);

		public int Reflect(int value)
		{
			return (int)((uint)Reverser.ReverseBytes(value) >> (32 - BitWidth));
		}
	}

}