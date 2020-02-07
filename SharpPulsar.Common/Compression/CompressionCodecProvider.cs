using SharpPulsar.Api;
using SharpPulsar.Common.Enum;
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
				[CompressionType.NONE] = new CompressionCodecNone(),
				[CompressionType.LZ4] = new CompressionCodecLZ4(),
				[CompressionType.ZLIB] = new CompressionCodecZLib(),
				[CompressionType.ZSTD] = new CompressionCodecZstd(),
				[CompressionType.SNAPPY] = new CompressionCodecSnappy()
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
			case ICompressionType.None:
				return CompressionType.NONE;
			case ICompressionType.Lz4:
				return CompressionType.LZ4;
			case ICompressionType.Zlib:
				return CompressionType.ZLIB;
			case ICompressionType.Zstd:
				return CompressionType.ZSTD;
			case ICompressionType.Snappy:
				return CompressionType.SNAPPY;

			default:
				throw new System.Exception("Invalid compression type");
			}
		}

		public static CompressionType? ConvertFromWireProtocol(CompressionType compressionType)
		{
			switch (compressionType)
			{
				case CompressionType.NONE:
					return CompressionType.NONE;
				case CompressionType.LZ4:
					return CompressionType.LZ4;
				case CompressionType.ZLIB:
					return CompressionType.ZLIB;
				case CompressionType.ZSTD:
					return CompressionType.ZSTD;
				case CompressionType.SNAPPY:
					return CompressionType.SNAPPY;

				default:
					throw new System.Exception("Invalid compression type");
			}
		}
	}

}