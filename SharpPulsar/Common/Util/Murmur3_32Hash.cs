/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
/*
 * The original MurmurHash3 was written by Austin Appleby, and is placed in the
 * public domain. This source code, implemented by Licht Takeuchi, is based on
 * the orignal MurmurHash3 source code.
 */

using System;

namespace SharpPulsar.Common.Util
{
    /// <summary>
	/// Implementation of the MurmurHash3 non-cryptographic hash function.
	/// </summary>
	public class Murmur332Hash : IHash
	{
		private static readonly Murmur332Hash instance = new Murmur332Hash();

		private const int ChunkSize = 4;
		private const int C1 = unchecked((int)0xcc9e2d51);
		private const int C2 = 0x1b873593;
		private readonly int seed;

		private Murmur332Hash()
		{
			seed = 0;
		}

		public static IHash Instance => instance;

        public int MakeHash(sbyte[] b)
		{
			return MakeHash0(b) & int.MaxValue;
		}

		private int MakeHash0(sbyte[] bytes)
        {
            var byt = (byte[]) (object) bytes;
			var len = bytes.Length;
			var reminder = len % ChunkSize;
			var h1 = seed;

			var byteBuffer = ByteBuffer.Allocate(len).Wrap(byt);
			//byteBuffer.Order(ByteOrder.LittleEndian);

			while (byteBuffer.Remaining() >= ChunkSize)
			{
				var kw = byteBuffer.GetInt();

				kw = MixK1(kw);
				h1 = MixH1(h1, kw);
			}

			var k1 = 0;
			for (var i = 0; i < reminder; i++)
			{
				k1 ^= (int)Convert.ToUInt32(byteBuffer.Get()) << (i * 8);
			}

			h1 ^= MixK1(k1);
			h1 ^= len;
			h1 = Fmix(h1);

			return h1;
		}

		private int Fmix(int h)
		{
			h ^= (int)((uint)h >> 16);
			h *= unchecked((int)0x85ebca6b);
			h ^= (int)((uint)h >> 13);
			h *= unchecked((int)0xc2b2ae35);
			h ^= (int)((uint)h >> 16);

			return h;
		}

		private int MixK1(int k1)
		{
			k1 *= C1;
			k1 = k1.RotateLeft(15);
			k1 *= C2;
			return k1;
		}

		private int MixH1(int h1, int k1)
		{
			h1 ^= k1;
			h1 = h1.RotateLeft(13);
			return h1 * 5 + unchecked((int)0xe6546b64);
		}

	}

    public static class Rotate
    {
        public static int RotateLeft(this int value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static int RotateRight(this int value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
        public static uint RotateLeft(this uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static uint RotateRight(this uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
	}
}