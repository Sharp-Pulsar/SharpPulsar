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
namespace SharpPulsar.Common.Compression
{
    using DotNetty.Buffers;
    using Microsoft.Extensions.Logging;
    using SharpPulsar.Common.Compression;
    using Snappy;
    using System.IO;

    //using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;

    /// <summary>
    /// Snappy Compression.
    /// </summary>
    public class CompressionCodecSnappy : CompressionCodec
	{
		public IByteBuffer Encode(IByteBuffer Source)
		{
			int UncompressedLength = Source.ReadableBytes;
			int MaxLength = SnappyCodec.GetMaxCompressedLength(UncompressedLength);

			var SourceNio = Source.GetIoBuffer(Source.ReaderIndex, Source.ReadableBytes).ToArray();

			var Target = PooledByteBufferAllocator.Default.Buffer(MaxLength, MaxLength);
			var TargetNio = Target.GetIoBuffer(0, MaxLength).ToArray();

			int CompressedLength = 0;
			try
			{
				CompressedLength = SnappyCodec.Compress(SourceNio, 0, SourceNio.Length, TargetNio, 0);
			}
			catch (IOException E)
			{
				log.LogError("Failed to compress to Snappy: {}", E.Message);
			}
			Target.SetWriterIndex(CompressedLength);
			return Target;
		}

		public IByteBuffer Decode(IByteBuffer Encoded, int UncompressedLength)
		{
			IByteBuffer Uncompressed = PooledByteBufferAllocator.Default.Buffer(UncompressedLength, UncompressedLength);
			var UncompressedNio = Uncompressed.GetIoBuffer(0, UncompressedLength).ToArray();

			var EncodedNio = Encoded.GetIoBuffer(Encoded.ReaderIndex, Encoded.ReadableBytes).ToArray();
			SnappyCodec.Uncompress(EncodedNio, 0, EncodedNio.Length, UncompressedNio, 0);

			Uncompressed.SetWriterIndex(UncompressedLength);
			return Uncompressed;
		}
		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(CompressionCodecSnappy));
	}
	
}