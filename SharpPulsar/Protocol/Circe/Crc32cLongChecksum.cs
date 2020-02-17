/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using DotNetty.Buffers;
using Microsoft.Extensions.Logging;

namespace SharpPulsar.Protocol.Circe
{

public class Crc32CLongChecksum
	{

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(Crc32CLongChecksum));

		internal static readonly IncrementalIntHash Crc32CHash;

		static Crc32CLongChecksum()
		{
            Crc32CHash = (new StandardCrcProvider()).GetIncrementalInt(CrcParameters.Crc32C);
            Log.LogWarning("Failed to load Circe JNI library. Falling back to Java based CRC32c provider");
		}

		/// <summary>
		/// Computes crc32c checksum: if it is able to load crc32c native library then it computes using that native library
		/// which is faster as it computes using hardware machine instruction else it computes using crc32c algo.
		/// </summary>
		/// <param name="payload">
		/// @return </param>
		public static long ComputeChecksum(IByteBuffer payload)
		{
			int crc;
			if (payload.HasArray)
			{
				crc = Crc32CHash.Calculate((sbyte[])(object)payload.Array, payload.ArrayOffset + payload.ReaderIndex, payload.ReadableBytes);
			}
			else
			{
				crc = Crc32CHash.Calculate((sbyte[])(object)payload.GetIoBuffer());
			}
			return crc & 0xffffffffL;
		}


		/// <summary>
		/// Computes incremental checksum with input previousChecksum and input payload
		/// </summary>
		/// <param name="previousChecksum"> : previously computed checksum </param>
		/// <param name="payload">
		/// @return </param>
		public static long ResumeChecksum(long previousChecksum, IByteBuffer payload)
		{
			int crc = (int) previousChecksum;
			if (payload.HasArray)
			{
				crc = Crc32CHash.Resume(crc, (sbyte[])(object)payload.Array, payload.ArrayOffset + payload.ReaderIndex, payload.ReadableBytes);
			}
			else
			{
				crc = Crc32CHash.Resume(crc, (sbyte[])(object)payload.GetIoBuffer());
			}
			return crc & 0xffffffffL;
		}

	}

}