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
namespace SharpPulsar.Util
{
	using UnsignedBytes = com.google.common.primitives.UnsignedBytes;

	/// <summary>
	/// Implementation of the MurmurHash3 non-cryptographic hash function.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:TypeName") public class Murmur3_32Hash implements Hash
	public class Murmur3_32Hash : Hash
	{
		private static readonly Murmur3_32Hash instance = new Murmur3_32Hash();

		private const int CHUNK_SIZE = 4;
		private const int C1 = unchecked((int)0xcc9e2d51);
		private const int C2 = 0x1b873593;
		private readonly int seed;

		private Murmur3_32Hash()
		{
			seed = 0;
		}

		public static Hash Instance
		{
			get
			{
				return instance;
			}
		}

		public virtual int makeHash(sbyte[] b)
		{
			return makeHash0(b) & int.MaxValue;
		}

		private int makeHash0(sbyte[] bytes)
		{
			int len = bytes.Length;
			int reminder = len % CHUNK_SIZE;
			int h1 = seed;

			ByteBuffer byteBuffer = ByteBuffer.wrap(bytes);
			byteBuffer.order(ByteOrder.LITTLE_ENDIAN);

			while (byteBuffer.remaining() >= CHUNK_SIZE)
			{
				int k1 = byteBuffer.Int;

				k1 = mixK1(k1);
				h1 = mixH1(h1, k1);
			}

			int k1 = 0;
			for (int i = 0; i < reminder; i++)
			{
				k1 ^= UnsignedBytes.toInt(byteBuffer.get()) << (i * 8);
			}

			h1 ^= mixK1(k1);
			h1 ^= len;
			h1 = fmix(h1);

			return h1;
		}

		private int fmix(int h)
		{
			h ^= (int)((uint)h >> 16);
			h *= unchecked((int)0x85ebca6b);
			h ^= (int)((uint)h >> 13);
			h *= unchecked((int)0xc2b2ae35);
			h ^= (int)((uint)h >> 16);

			return h;
		}

		private int mixK1(int k1)
		{
			k1 *= C1;
			k1 = Integer.rotateLeft(k1, 15);
			k1 *= C2;
			return k1;
		}

		private int mixH1(int h1, int k1)
		{
			h1 ^= k1;
			h1 = Integer.rotateLeft(h1, 13);
			return h1 * 5 + unchecked((int)0xe6546b64);
		}
	}

}