﻿

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
/// 
using K4os.Compression.LZ4;

namespace SharpPulsar.Common.Compression
{

	/// <summary>
	/// LZ4 Compression.
	/// </summary>
	public class CompressionCodecLz4 : CompressionCodec
	{

		static CompressionCodecLz4()
		{
			
		}

		//private static readonly LZ4Factory lz4Factory = LZ4Factory.fastestInstance();
		//private static readonly LZ4Compressor compressor = lz4Factory.fastCompressor();
		//private static readonly LZ4FastDecompressor decompressor = lz4Factory.fastDecompressor();

		public byte[] Encode(byte[] source)
		{
			var lz4 = new CompressionCodecLz4();
			int uncompressedLength = source.Length;
			int maxLength = LZ4Codec.MaximumOutputSize(uncompressedLength);

			var sourceNio = source;

            byte[] target = new byte[maxLength];
			var targetNio = target;

			int compressedLength = LZ4Codec.Encode(sourceNio, 0, uncompressedLength, targetNio, 0, maxLength);
			//target.SetWriterIndex(compressedLength);
			return target;
		}

		public byte[] Decode(byte[] encoded, int uncompressedLength)
		{
			byte[] uncompressed = new byte[uncompressedLength];
			var uncompressedNio = uncompressed;

			var encodedNio = encoded;
			LZ4Codec.Decode(encodedNio, 0,  encodedNio.Length, uncompressedNio, 0, uncompressedLength);

			//uncompressed.SetWriterIndex(uncompressedLength);
			return uncompressed;
		}
	}

}