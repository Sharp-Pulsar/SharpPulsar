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

using System.Text;
using SharpPulsar.Api;
using SharpPulsar.Common;

namespace SharpPulsar.Impl
{

	public class JavaStringHash : IHash
	{
		private static readonly JavaStringHash instance = new JavaStringHash();

		private JavaStringHash()
		{
		}

		public static IHash Instance => instance;

        public int MakeHash(string s)
		{
			return s.GetHashCode() & int.MaxValue;
		}

		public  int MakeHash(sbyte[] b)
		{
			return MakeHash(StringHelper.NewString(b, Encoding.UTF8.EncodingName));
		}

	}

}