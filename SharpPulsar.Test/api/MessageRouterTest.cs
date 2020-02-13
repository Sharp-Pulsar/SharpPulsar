using System;

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
namespace SharpPulsar.Test
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test of <seealso cref="MessageRouter"/>.
	/// </summary>
	public class MessageRouterTest
	{

		[Serializable]
		public class TestMessageRouter : MessageRouter
		{

			public override int ChoosePartition<T1>(Message<T1> Msg)
			{
				return 1234;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @Test public void testChoosePartition()
		public virtual void TestChoosePartition()
		{
			MessageRouter Router = spy(new TestMessageRouter());
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Message<?> mockedMsg = mock(Message.class);
			Message<object> MockedMsg = mock(typeof(Message));
			TopicMetadata MockedMetadata = mock(typeof(TopicMetadata));

			assertEquals(1234, Router.choosePartition(MockedMsg));
			assertEquals(1234, Router.choosePartition(MockedMsg, MockedMetadata));

			verify(Router, times(2)).choosePartition(eq(MockedMsg));
		}

	}

}