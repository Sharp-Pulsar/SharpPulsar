using SharpPulsar.Protocol.Proto;
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
		private static readonly Dictionary<Protocol.Proto.CompressionType, CompressionCodec> Codecs;

		static CompressionCodecProvider()
		{
			Codecs = new Dictionary<Protocol.Proto.CompressionType, CompressionCodec>
			{
				[CompressionType.None] = new CompressionCodecNone(),
				[CompressionType.Lz4] = new CompressionCodecLz4(),
				[CompressionType.Zlib] = new CompressionCodecZLib(),
				[CompressionType.Zstd] = new CompressionCodecZstd(),
				[CompressionType.Snappy] = new CompressionCodecSnappy()
			};
		}

        public static CompressionCodec GetCompressionCodec(int type)
        {
            return Codecs[ConvertToWireProtocol(type)];
        }
		public static CompressionCodec GetCompressionCodec(CompressionType type)
		{
			return Codecs[ConvertToWireProtocol(type)];
		}

		public static CompressionType ConvertToWireProtocol(CompressionType compressionType)
		{
			return compressionType;
		}
        public static CompressionType ConvertToWireProtocol(int compressionType)
        {

            switch (compressionType)
            {
                case 0:
                    return CompressionType.None;
                case 1:
                    return CompressionType.Lz4;
                case 2:
                    return CompressionType.Zlib;
                case 3:
                    return CompressionType.Zstd;
                case 4:
                    return CompressionType.Snappy;

                default:
                    throw new System.Exception("Invalid compression type");
            }
        }

	}

}