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
namespace org.apache.pulsar.client.api
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;

	using Test = org.testng.annotations.Test;

	using Objects = com.google.common.@base.Objects;

	using ConsumerId = impl.ConsumerId;

	public class ConsumerIdTest
	{
		private const string TOPIC_TEST = "my-topic-1";
		private const string TOPIC_TEST_2 = "my-topic-2";
		private const string SUBCRIBTION_TEST = "my-sub-1";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getTopicTest()
		public virtual void getTopicTest()
		{
			ConsumerId testConsumerId = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			assertEquals(TOPIC_TEST, testConsumerId.Topic);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getSubscribtionTest()
		public virtual void getSubscribtionTest()
		{
			ConsumerId testConsumerId = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			assertEquals(SUBCRIBTION_TEST, testConsumerId.Subscription);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hashCodeTest()
		public virtual void hashCodeTest()
		{
			ConsumerId testConsumerId = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			assertEquals(Objects.hashCode(TOPIC_TEST, SUBCRIBTION_TEST), testConsumerId.GetHashCode());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalTest()
		public virtual void equalTest()
		{
			ConsumerId testConsumerId1 = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			ConsumerId testConsumerId2 = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			ConsumerId testConsumerId3 = new ConsumerId(TOPIC_TEST_2, SUBCRIBTION_TEST);

			assertEquals(testConsumerId2, testConsumerId1);

			assertNotEquals(testConsumerId3, testConsumerId1);

			assertNotEquals("", testConsumerId1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToTest()
		public virtual void compareToTest()
		{
			ConsumerId testConsumerId1 = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			ConsumerId testConsumerId2 = new ConsumerId(TOPIC_TEST, SUBCRIBTION_TEST);
			ConsumerId testConsumerId3 = new ConsumerId(TOPIC_TEST_2, SUBCRIBTION_TEST);

			assertEquals(0, testConsumerId1.CompareTo(testConsumerId2));
			assertEquals(-1, testConsumerId1.CompareTo(testConsumerId3));

		}
	}

}