using SharpPulsar.Api;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;

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
	/// Provider of compression codecs used in Pulsar.
	/// </summary>
	/// <seealso cref= CompressionCodecNone </seealso>
	/// <seealso cref= CompressionCodecLZ4 </seealso>
	/// <seealso cref= CompressionCodecZLib </seealso>
	/// <seealso cref= CompressionCodecZstd </seealso>
	/// <seealso cref= CompressionCodecSnappy </seealso>
	/// 
	public static class CompressionCodecProvider
	{
		private static readonly Dictionary<CompressionType, CompressionCodec> codecs;

		static CompressionCodecProvider()
		{
			codecs = new Dictionary<CompressionType, CompressionCodec>
			{
				[CompressionType.None] = new CompressionCodecNone(),
				[CompressionType.Lz4] = new CompressionCodecLZ4(),
				[CompressionType.Zlib] = new CompressionCodecZLib(),
				[CompressionType.Zstd] = new CompressionCodecZstd(),
				[CompressionType.Snappy] = new CompressionCodecSnappy()
			};
		}

		public static CompressionCodec GetCompressionCodec(CompressionType type)
		{
			return codecs[type];
		}

		public static CompressionCodec GetCompressionCodec(ICompressionType type)
		{
			return codecs[ConvertToWireProtocol(type)];
		}

		public static CompressionType ConvertToWireProtocol(ICompressionType compressionType)
		{
			
			switch (compressionType)
			{
			case ICompressionType.NONE:
				return CompressionType.None;
			case ICompressionType.LZ4:
				return CompressionType.Lz4;
			case ICompressionType.ZLIB:
				return CompressionType.Zlib;
			case ICompressionType.ZSTD:
				return CompressionType.Zstd;
			case ICompressionType.SNAPPY:
				return CompressionType.Snappy;

			default:
				throw new Exception("Invalid compression type");
			}
		}

		public static CompressionType? ConvertFromWireProtocol(CompressionType compressionType)
		{
			switch (compressionType)
			{
			case CompressionType.None:
				return CompressionType.None;
			case CompressionType.Lz4:
				return CompressionType.Lz4;
			case CompressionType.Zlib:
				return CompressionType.Zlib;
			case CompressionType.Zstd:
				return CompressionType.Zstd;
			case CompressionType.Snappy:
				return CompressionType.Snappy;

			default:
				throw new Exception("Invalid compression type");
			}
		}
	}

}