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
	using Zstd = com.github.luben.zstd.Zstd;

	using ByteBuf = io.netty.buffer.ByteBuf;

	using PulsarByteBufAllocator = Org.Apache.Pulsar.Common.Allocator.PulsarByteBufAllocator;
    using SharpPulsar.Common.Compression;

    /// <summary>
    /// Zstandard Compression.
    /// </summary>
    public class CompressionCodecZstd : CompressionCodec
	{

		private const int ZstdCompressionLevel = 3;

		public override ByteBuf Encode(ByteBuf Source)
		{
			int UncompressedLength = Source.readableBytes();
			int MaxLength = (int) Zstd.compressBound(UncompressedLength);

			ByteBuf Target = PulsarByteBufAllocator.DEFAULT.directBuffer(MaxLength, MaxLength);
			int CompressedLength;

			if (Source.hasMemoryAddress())
			{
				CompressedLength = (int) Zstd.compressUnsafe(Target.memoryAddress(), MaxLength, Source.memoryAddress() + Source.readerIndex(), UncompressedLength, ZstdCompressionLevel);
			}
			else
			{
				ByteBuffer SourceNio = Source.nioBuffer(Source.readerIndex(), Source.readableBytes());
				ByteBuffer TargetNio = Target.nioBuffer(0, MaxLength);

				CompressedLength = Zstd.compress(TargetNio, SourceNio, ZstdCompressionLevel);
			}

			Target.writerIndex(CompressedLength);
			return Target;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public io.netty.buffer.ByteBuf decode(io.netty.buffer.ByteBuf encoded, int uncompressedLength) throws java.io.IOException
		public override ByteBuf Decode(ByteBuf Encoded, int UncompressedLength)
		{
			ByteBuf Uncompressed = PulsarByteBufAllocator.DEFAULT.directBuffer(UncompressedLength, UncompressedLength);

			if (Encoded.hasMemoryAddress())
			{
				Zstd.decompressUnsafe(Uncompressed.memoryAddress(), UncompressedLength, Encoded.memoryAddress() + Encoded.readerIndex(), Encoded.readableBytes());
			}
			else
			{
				ByteBuffer UncompressedNio = Uncompressed.nioBuffer(0, UncompressedLength);
				ByteBuffer EncodedNio = Encoded.nioBuffer(Encoded.readerIndex(), Encoded.readableBytes());

				Zstd.decompress(UncompressedNio, EncodedNio);
			}

			Uncompressed.writerIndex(UncompressedLength);
			return Uncompressed;
		}
	}

}