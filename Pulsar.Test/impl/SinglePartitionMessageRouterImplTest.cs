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
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using HashingScheme = org.apache.pulsar.client.api.HashingScheme;
	using Message = org.apache.pulsar.client.api.Message;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test of <seealso cref="SinglePartitionMessageRouterImpl"/>.
	/// </summary>
	public class SinglePartitionMessageRouterImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithoutKey()
		public virtual void testChoosePartitionWithoutKey()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg = mock(typeof(Message));
			when(msg.Key).thenReturn(null);

			SinglePartitionMessageRouterImpl router = new SinglePartitionMessageRouterImpl(1234, HashingScheme.JavaStringHash);
			assertEquals(1234, router.choosePartition(msg, new TopicMetadataImpl(2468)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithKey()
		public virtual void testChoosePartitionWithKey()
		{
			string key1 = "key1";
			string key2 = "key2";
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg1 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg1 = mock(typeof(Message));
			when(msg1.hasKey()).thenReturn(true);
			when(msg1.Key).thenReturn(key1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg2 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg2 = mock(typeof(Message));
			when(msg2.hasKey()).thenReturn(true);
			when(msg2.Key).thenReturn(key2);

			SinglePartitionMessageRouterImpl router = new SinglePartitionMessageRouterImpl(1234, HashingScheme.JavaStringHash);
			TopicMetadataImpl metadata = new TopicMetadataImpl(100);

			assertEquals(key1.GetHashCode() % 100, router.choosePartition(msg1, metadata));
			assertEquals(key2.GetHashCode() % 100, router.choosePartition(msg2, metadata));
		}

	}

}