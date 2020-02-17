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
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;

/// <summary>
	/// Unit test of <seealso cref="Message"/> methods.
	/// </summary>
	public class MessageTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageImplReplicatedInfo()
		public virtual void TestMessageImplReplicatedInfo()
		{
			string From = "ClusterNameOfReplicatedFrom";
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setReplicatedFrom(From);
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
			Message<sbyte[]> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertTrue(Msg.Replicated);
			assertEquals(Msg.ReplicatedFrom, From);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageImplNoReplicatedInfo()
		public virtual void TestMessageImplNoReplicatedInfo()
		{
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder();
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
			Message<sbyte[]> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertFalse(Msg.Replicated);
			assertTrue(Msg.ReplicatedFrom.Length == 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTopicMessageImplReplicatedInfo()
		public virtual void TestTopicMessageImplReplicatedInfo()
		{
			string From = "ClusterNameOfReplicatedFromForTopicMessage";
			string TopicName = "myTopic";
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setReplicatedFrom(From);
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
			MessageImpl<sbyte[]> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);
			Msg.setMessageId(new MessageIdImpl(-1, -1, -1));
			TopicMessageImpl<sbyte[]> TopicMessage = new TopicMessageImpl<sbyte[]>(TopicName, TopicName, Msg);

			assertTrue(TopicMessage.Replicated);
			assertEquals(Msg.ReplicatedFrom, From);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTopicMessageImplNoReplicatedInfo()
		public virtual void TestTopicMessageImplNoReplicatedInfo()
		{
			string TopicName = "myTopic";
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder();
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
			MessageImpl<sbyte[]> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);
			Msg.setMessageId(new MessageIdImpl(-1, -1, -1));
			TopicMessageImpl<sbyte[]> TopicMessage = new TopicMessageImpl<sbyte[]>(TopicName, TopicName, Msg);

			assertFalse(TopicMessage.Replicated);
			assertTrue(TopicMessage.ReplicatedFrom.Length == 0);
		}
	}

}