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

using ZstdNet;

namespace SharpPulsar.Common.Compression
{
    /// <summary>
    /// Zstandard Compression.
    /// </summary>
    public class CompressionCodecZstd : CompressionCodec
	{

		private const int ZstdCompressionLevel = 3;

		public byte[] Encode(byte[] source)
		{
            var compressor = new Compressor();
			
			/*if (source.HasMemoryAddress)
			{
				compressedLength = (int) compressor. . Zstd.compressUnsafe(target.GetPinnableMemoryAddress(), maxLength, source.GetPinnableMemoryAddress() + source.ReaderIndex, uncompressedLength, ZstdCompressionLevel);
			}
			else
			{
				var sourceNio = source.GetIoBuffer(source.ReaderIndex, source.ReadableBytes);
				var targetNio = target.GetIoBuffer(0, maxLength);

				compressedLength = Zstd.compress(targetNio, sourceNio, ZstdCompressionLevel);
			}*/
            var sourceNio = source;
            var compressed = compressor.Wrap(sourceNio);

			return compressed;
		}

		public byte[] Decode(byte[] encoded, int uncompressedLength)
		{
			var decompressor = new Decompressor();
			/*if (encoded.HasMemoryAddress)
			{
				Zstd.decompressUnsafe(uncompressed.GetPinnableMemoryAddress(), uncompressedLength, encoded.GetPinnableMemoryAddress() + encoded.ReaderIndex, encoded.ReadableBytes);
			}
			else
			{
				var uncompressedNio = uncompressed.GetIoBuffer(0, uncompressedLength);
				var encodedNio = encoded.GetIoBuffer(encoded.ReaderIndex, encoded.ReadableBytes);

				Zstd.decompress(uncompressedNio, encodedNio);
			}*/
            
           var rt = decompressor.Unwrap(encoded, uncompressedLength);
			return rt;
		}
	}

}