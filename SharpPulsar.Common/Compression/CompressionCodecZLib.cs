using DotNetty.Buffers;
using DotNetty.Common;
using ICSharpCode.SharpZipLib.Zip.Compression;
using SharpPulsar.Common.Allocator;
using SharpPulsar.Common.Compression;
using System;

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


	/// <summary>
	/// ZLib Compression.
	/// </summary>
	public class CompressionCodecZLib : CompressionCodec
	{

		private readonly FastThreadLocal<Deflater> _deflater = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<Deflater>
		{
			public Deflater InitialValue()
			{
				return new Deflater();
			}

			public void OnRemoval(Deflater deflater)
			{
				deflater.Finish();
			}
		}

		private readonly FastThreadLocal<Inflater> _inflater = new FastThreadLocalAnonymousInnerClass2();

		public class FastThreadLocalAnonymousInnerClass2 : FastThreadLocal<Inflater>
		{
			public Inflater InitialValue()
			{
				return new Inflater();
			}

			public void OnRemoval(Inflater inflater)
			{
				//inflater.se.IsFinished(true);
			}
		}

		public IByteBuffer Encode(IByteBuffer source)
		{
			sbyte[] array;
			int length = source.ReadableBytes;

			int sizeEstimate = (int) Math.Ceiling(source.ReadableBytes * 1.001) + 14;
			IByteBuffer compressed = PulsarByteBufAllocator.DEFAULT.HeapBuffer(sizeEstimate);

			int offset = 0;
			if (source.HasArray)
			{
				array = (sbyte[])(object)source.Array;
				offset = source.ArrayOffset + source.ReaderIndex;
			}
			else
			{
				// If it's a direct buffer, we need to copy it
				array = new sbyte[length];
				source.GetBytes(source.ReaderIndex, array);
			}

			Deflater deflater = _deflater.Get(this);
			deflater.Reset();
			deflater.SetInput((byte[])(object)array, offset, length);
			while (!deflater.IsNeedingInput)
			{
				Deflate(deflater, compressed);
			}

			return compressed;
		}

		private static void Deflate(Deflater deflater, IByteBuffer @out)
		{
			int numBytes;
			do
			{
				int writerIndex = @out.WriterIndex;
				numBytes = deflater.Deflate(@out.Array, @out.ArrayOffset + writerIndex, @out.WritableBytes,deflater.Flush);
				@out.WriterIndex = writerIndex + numBytes;
			} while (numBytes > 0);
		}

		public override ByteBuf Decode(ByteBuf Encoded, int UncompressedLength)
		{
			ByteBuf Uncompressed = PulsarByteBufAllocator.DEFAULT.heapBuffer(UncompressedLength, UncompressedLength);

			int Len = Encoded.readableBytes();

			sbyte[] Array;
			int Offset;
			if (Encoded.hasArray())
			{
				Array = Encoded.array();
				Offset = Encoded.arrayOffset() + Encoded.readerIndex();
			}
			else
			{
				Array = new sbyte[Len];
				Encoded.getBytes(Encoded.readerIndex(), Array);
				Offset = 0;
			}

			int ResultLength;
			Inflater Inflater = this.inflater.get();
			Inflater.reset();
			Inflater.setInput(Array, Offset, Len);

			try
			{
				ResultLength = Inflater.inflate(Uncompressed.array(), Uncompressed.arrayOffset(), UncompressedLength);
			}
			catch (DataFormatException E)
			{
				throw new IOException(E);
			}

			checkArgument(ResultLength == UncompressedLength);

			Uncompressed.writerIndex(UncompressedLength);
			return Uncompressed;
		}
	}

}