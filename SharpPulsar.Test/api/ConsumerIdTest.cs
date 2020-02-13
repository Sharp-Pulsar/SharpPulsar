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
namespace Org.Apache.Pulsar.Client.Api
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;

	using Test = org.testng.annotations.Test;

	using Objects = com.google.common.@base.Objects;

	using ConsumerId = Org.Apache.Pulsar.Client.Impl.ConsumerId;

	public class ConsumerIdTest
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private const string TopicTest_Conflict = "my-topic-1";
		private const string TopicTest_2 = "my-topic-2";
		private const string SubcribtionTest = "my-sub-1";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getTopicTest()
		public virtual void getTopicTest()
		{
			ConsumerId TestConsumerId = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			assertEquals(TopicTest_Conflict, TestConsumerId.Topic);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getSubscribtionTest()
		public virtual void getSubscribtionTest()
		{
			ConsumerId TestConsumerId = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			assertEquals(SubcribtionTest, TestConsumerId.Subscription);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hashCodeTest()
		public virtual void HashCodeTest()
		{
			ConsumerId TestConsumerId = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			assertEquals(Objects.hashCode(TopicTest_Conflict, SubcribtionTest), TestConsumerId.GetHashCode());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalTest()
		public virtual void EqualTest()
		{
			ConsumerId TestConsumerId1 = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			ConsumerId TestConsumerId2 = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			ConsumerId TestConsumerId3 = new ConsumerId(TopicTest_2, SubcribtionTest);

			assertEquals(TestConsumerId2, TestConsumerId1);

			assertNotEquals(TestConsumerId3, TestConsumerId1);

			assertNotEquals("", TestConsumerId1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToTest()
		public virtual void CompareToTest()
		{
			ConsumerId TestConsumerId1 = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			ConsumerId TestConsumerId2 = new ConsumerId(TopicTest_Conflict, SubcribtionTest);
			ConsumerId TestConsumerId3 = new ConsumerId(TopicTest_2, SubcribtionTest);

			assertEquals(0, TestConsumerId1.CompareTo(TestConsumerId2));
			assertEquals(-1, TestConsumerId1.CompareTo(TestConsumerId3));

		}
	}

}