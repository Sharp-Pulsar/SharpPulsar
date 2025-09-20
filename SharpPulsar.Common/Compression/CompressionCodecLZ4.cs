

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
/// 

using System.Linq;
using K4os.Compression.LZ4;

namespace SharpPulsar.Common.Compression
{

	/// <summary>
	/// LZ4 Compression.
	/// </summary>
	public class CompressionCodecLz4 : CompressionCodec
	{

		static CompressionCodecLz4()
		{
			
		}
		
		public byte[] Encode(byte[] source)
		{
			var maxLength = LZ4Codec.MaximumOutputSize(source.Length);

            var target = new byte[maxLength];

			var count = LZ4Codec.Encode(source, 0, source.Length, target, 0, target.Length);
			
			return target.Take(count).ToArray();
		}

		public byte[] Decode(byte[] encoded, int uncompressedLength)
        {
            var target = new byte[uncompressedLength];
			LZ4Codec.Decode(encoded, 0,  encoded.Length, target, 0, target.Length);
			return target;
		}
	}

}