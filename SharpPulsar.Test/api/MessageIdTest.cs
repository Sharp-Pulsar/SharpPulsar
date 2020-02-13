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

	using BatchMessageIdImpl = Org.Apache.Pulsar.Client.Impl.BatchMessageIdImpl;
	using MessageIdImpl = Org.Apache.Pulsar.Client.Impl.MessageIdImpl;
	using Test = org.testng.annotations.Test;

	public class MessageIdTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void messageIdTest()
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		public virtual void MessageIdTestConflict()
		{
			MessageId MId = new MessageIdImpl(1, 2, 3);
			assertEquals(MId.ToString(), "1:2:3");

			MId = new BatchMessageIdImpl(0, 2, 3, 4);
			assertEquals(MId.ToString(), "0:2:3:4");

			MId = new BatchMessageIdImpl(-1, 2, -3, 4);
			assertEquals(MId.ToString(), "-1:2:-3:4");

			MId = new MessageIdImpl(0, -23, 3);
			assertEquals(MId.ToString(), "0:-23:3");
		}
	}

}