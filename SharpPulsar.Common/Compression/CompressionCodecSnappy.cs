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
    using SharpPulsar.Common.Compression;
    using ByteBuf = io.netty.buffer.ByteBuf;
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Snappy = org.xerial.snappy.Snappy;

	/// <summary>
	/// Snappy Compression.
	/// </summary>
	public class CompressionCodecSnappy : CompressionCodec
	{
		public override ByteBuf Encode(ByteBuf Source)
		{
			int UncompressedLength = Source.readableBytes();
			int MaxLength = Snappy.maxCompressedLength(UncompressedLength);

			ByteBuffer SourceNio = Source.nioBuffer(Source.readerIndex(), Source.readableBytes());

			ByteBuf Target = PooledByteBufAllocator.DEFAULT.buffer(MaxLength, MaxLength);
			ByteBuffer TargetNio = Target.nioBuffer(0, MaxLength);

			int CompressedLength = 0;
			try
			{
				CompressedLength = Snappy.compress(SourceNio, TargetNio);
			}
			catch (IOException E)
			{
				log.error("Failed to compress to Snappy: {}", E.Message);
			}
			Target.writerIndex(CompressedLength);
			return Target;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public io.netty.buffer.ByteBuf decode(io.netty.buffer.ByteBuf encoded, int uncompressedLength) throws java.io.IOException
		public override ByteBuf Decode(ByteBuf Encoded, int UncompressedLength)
		{
			ByteBuf Uncompressed = PooledByteBufAllocator.DEFAULT.buffer(UncompressedLength, UncompressedLength);
			ByteBuffer UncompressedNio = Uncompressed.nioBuffer(0, UncompressedLength);

			ByteBuffer EncodedNio = Encoded.nioBuffer(Encoded.readerIndex(), Encoded.readableBytes());
			Snappy.uncompress(EncodedNio, UncompressedNio);

			Uncompressed.writerIndex(UncompressedLength);
			return Uncompressed;
		}
	}

}