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
namespace Org.Apache.Pulsar.Client.Impl
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
		public virtual void CompareToTest()
		{
			BatchMessageIdImpl BatchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl BatchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);

			assertEquals(BatchMsgId1.CompareTo(BatchMsgId2), -1);
			assertEquals(BatchMsgId2.CompareTo(BatchMsgId1), 1);
			assertEquals(BatchMsgId2.CompareTo(BatchMsgId2), 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hashCodeTest()
		public virtual void HashCodeTest()
		{
			BatchMessageIdImpl BatchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl BatchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);

			assertEquals(BatchMsgId1.GetHashCode(), BatchMsgId1.GetHashCode());
			assertTrue(BatchMsgId1.GetHashCode() != BatchMsgId2.GetHashCode());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void EqualsTest()
		{
			BatchMessageIdImpl BatchMsgId1 = new BatchMessageIdImpl(0, 0, 0, 0);
			BatchMessageIdImpl BatchMsgId2 = new BatchMessageIdImpl(1, 1, 1, 1);
			BatchMessageIdImpl BatchMsgId3 = new BatchMessageIdImpl(0, 0, 0, 1);
			BatchMessageIdImpl BatchMsgId4 = new BatchMessageIdImpl(0, 0, 0, -1);
			MessageIdImpl MsgId = new MessageIdImpl(0, 0, 0);

			assertEquals(BatchMsgId1, BatchMsgId1);
			assertNotEquals(BatchMsgId2, BatchMsgId1);
			assertNotEquals(BatchMsgId3, BatchMsgId1);
			assertNotEquals(BatchMsgId4, BatchMsgId1);
			assertNotEquals(MsgId, BatchMsgId1);

			assertEquals(MsgId, MsgId);
			assertNotEquals(BatchMsgId1, MsgId);
			assertNotEquals(BatchMsgId2, MsgId);
			assertNotEquals(BatchMsgId3, MsgId);
			assertEquals(BatchMsgId4, MsgId);

			assertEquals(MsgId, BatchMsgId4);
		}

	}

}