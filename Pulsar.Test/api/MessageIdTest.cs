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

	using BatchMessageIdImpl = org.apache.pulsar.client.impl.BatchMessageIdImpl;
	using MessageIdImpl = org.apache.pulsar.client.impl.MessageIdImpl;
	using Test = org.testng.annotations.Test;

	public class MessageIdTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void messageIdTest()
		public virtual void messageIdTest()
		{
			MessageId mId = new MessageIdImpl(1, 2, 3);
			assertEquals(mId.ToString(), "1:2:3");

			mId = new BatchMessageIdImpl(0, 2, 3, 4);
			assertEquals(mId.ToString(), "0:2:3:4");

			mId = new BatchMessageIdImpl(-1, 2, -3, 4);
			assertEquals(mId.ToString(), "-1:2:-3:4");

			mId = new MessageIdImpl(0, -23, 3);
			assertEquals(mId.ToString(), "0:-23:3");
		}
	}

}