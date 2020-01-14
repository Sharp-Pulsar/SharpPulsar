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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using Test = org.testng.annotations.Test;

	public class BatchMessageIdImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToTest()
		public virtual void compareToTest()
		{
			BatchMessageIdImpl batchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl batchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);

			assertEquals(batchMsgId1.compareTo(batchMsgId2), -1);
			assertEquals(batchMsgId2.compareTo(batchMsgId1), 1);
			assertEquals(batchMsgId2.compareTo(batchMsgId2), 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hashCodeTest()
		public virtual void hashCodeTest()
		{
			BatchMessageIdImpl batchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl batchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);

			assertEquals(batchMsgId1.GetHashCode(), batchMsgId1.GetHashCode());
			assertTrue(batchMsgId1.GetHashCode() != batchMsgId2.GetHashCode());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			BatchMessageIdImpl batchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl batchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);
			BatchMessageIdImpl batchMsgId3 = new BatchMessageIdImpl(0, 0, 0, 1);
			BatchMessageIdImpl batchMsgId4 = new BatchMessageIdImpl(0, 0, 0, -1);
			MessageIdImpl msgId = new MessageIdImpl(0, 0, 0);

			assertEquals(batchMsgId1, batchMsgId1);
			assertNotEquals(batchMsgId2, batchMsgId1);
			assertNotEquals(batchMsgId3, batchMsgId1);
			assertNotEquals(batchMsgId4, batchMsgId1);
			assertNotEquals(msgId, batchMsgId1);

			assertEquals(msgId, msgId);
			assertNotEquals(batchMsgId1, msgId);
			assertNotEquals(batchMsgId2, msgId);
			assertNotEquals(batchMsgId3, msgId);
			assertEquals(batchMsgId4, msgId);

			assertEquals(msgId, batchMsgId4);
		}

	}

}