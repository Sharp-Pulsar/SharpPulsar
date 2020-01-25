using DotNetty.Buffers;
using SharpPulsar.Common.Compression;
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
	/// No compression.
	/// </summary>
	public class CompressionCodecNone : CompressionCodec
	{

		public IByteBuffer Encode(IByteBuffer raw)
		{
			// Provides an encoder that simply returns the same uncompressed buffer
			raw.Retain();
			return raw;
		}

		public IByteBuffer Decode(IByteBuffer encoded, int UncompressedSize)
		{
			// No decompression is required for this codec
			encoded.Retain();
			return encoded;
		}
	}

}