using Org.Apache.Pulsar.Common.Util;

using System.Text;

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
/*
 * The original MurmurHash3 was written by Austin Appleby, and is placed in the
 * public domain. This source code, implemented by Licht Takeuchi, is based on
 * the orignal MurmurHash3 source code.
 */
namespace SharpPulsar.Impl
{

	public class Murmur3_32Hash : Hash
	{
		private static readonly Murmur3_32Hash instance = new Murmur3_32Hash();

		private Murmur3_32Hash()
		{
		}

		public static Hash Instance
		{
			get
			{
				return instance;
			}
		}

		public override int MakeHash(string S)
		{
			return Murmur3_32Hash.Instance.makeHash(S.GetBytes(Encoding.UTF8)) & int.MaxValue;
		}

		public override int MakeHash(sbyte[] B)
		{
			return Murmur3_32Hash.Instance.makeHash(B) & int.MaxValue;
		}
	}

}