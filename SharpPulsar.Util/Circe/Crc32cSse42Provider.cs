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

using System.Collections.Generic;

namespace SharpPulsar.Util.Circe
{
    public sealed class Crc32CSse42Provider : AbstractHashProvider<HashParameters>
	{

		// Default chunks of 32 KB, then 4 KB, then 512 bytes
		private static readonly int[] DefaultChunk = new int[] {4096, 512, 64};

		public Crc32CSse42Provider() : base(typeof(HashParameters))
		{
		}

		public override ISet<HashSupport> QuerySupportTyped(HashParameters @params)
		{
			if (IsCrc32C(@params) && Circe.Sse42Crc32C.Supported)
			{
				return (ISet<HashSupport>) (typeof(HashSupport)).MakeArrayType();
			}
			return new HashSet<HashSupport>();
		}

		public override Hash Get(HashParameters @params, ISet<HashSupport> required)
		{
			if (IsCrc32C(@params) && Circe.Sse42Crc32C.Supported)
			{
				return GetCacheable(@params, required);
			}
			throw new System.NotSupportedException();
		}

		private static bool IsCrc32C(HashParameters @params)
		{
			return @params.Equals(Circe.CrcParameters.Crc32C);
		}

		public override StatelessHash CreateCacheable(HashParameters @params, ISet<HashSupport> required)
		{
			return new Circe.Sse42Crc32C(DefaultChunk);
		}
	}

}