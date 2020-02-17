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
namespace SharpPulsar.Test.Impl
{
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.*;

	public class HashTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void javaStringHashTest()
		public virtual void JavaStringHashTest()
		{
			Hash H = JavaStringHash.Instance;

			// Calculating `hashCode()` makes overflow as unsigned int32.
			string Key1 = "keykeykeykeykey1";

			// `hashCode()` is negative as signed int32.
			string Key2 = "keykeykey2";

			// Same value as C++ client
			assertEquals(434058482, H.makeHash(Key1));
			assertEquals(42978643, H.makeHash(Key2));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void murmur3_32HashTest()
		public virtual void Murmur3_32HashTest()
		{
			Hash H = Murmur3_32Hash.Instance;

			// Same value as C++ client
			assertEquals(2110152746, H.makeHash("k1"));
			assertEquals(1479966664, H.makeHash("k2"));
			assertEquals(462881061, H.makeHash("key1"));
			assertEquals(1936800180, H.makeHash("key2"));
			assertEquals(39696932, H.makeHash("key01"));
			assertEquals(751761803, H.makeHash("key02"));
		}
	}

}