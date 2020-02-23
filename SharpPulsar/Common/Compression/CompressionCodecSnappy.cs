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

using DotNetty.Buffers;

namespace SharpPulsar.Common.Compression
{
    using Microsoft.Extensions.Logging;
    using Snappy;
    using System.IO;

    //using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;

    /// <summary>
    /// Snappy Compression.
    /// </summary>
    public class CompressionCodecSnappy : CompressionCodec
	{
		public IByteBuffer Encode(IByteBuffer source)
		{
			int uncompressedLength = source.ReadableBytes;
			int maxLength = SnappyCodec.GetMaxCompressedLength(uncompressedLength);

			var sourceNio = source.GetIoBuffer(source.ReaderIndex, source.ReadableBytes).ToArray();

			var target = PooledByteBufferAllocator.Default.Buffer(maxLength, maxLength);
			var targetNio = target.GetIoBuffer(0, maxLength).ToArray();

			int compressedLength = 0;
			try
			{
				compressedLength = SnappyCodec.Compress(sourceNio, 0, sourceNio.Length, targetNio, 0);
			}
			catch (IOException e)
			{
				Log.LogError("Failed to compress to Snappy: {}", e.Message);
			}
			target.SetWriterIndex(compressedLength);
			return target;
		}

		public IByteBuffer Decode(IByteBuffer encoded, int uncompressedLength)
		{
			IByteBuffer uncompressed = PooledByteBufferAllocator.Default.Buffer(uncompressedLength, uncompressedLength);
			var uncompressedNio = uncompressed.GetIoBuffer(0, uncompressedLength).ToArray();

			var encodedNio = encoded.GetIoBuffer(encoded.ReaderIndex, encoded.ReadableBytes).ToArray();
			SnappyCodec.Uncompress(encodedNio, 0, encodedNio.Length, uncompressedNio, 0);

			uncompressed.SetWriterIndex(uncompressedLength);
			return uncompressed;
		}
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(CompressionCodecSnappy));
	}
	
}