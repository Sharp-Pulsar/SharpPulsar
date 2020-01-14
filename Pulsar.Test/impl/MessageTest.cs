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
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using Message = org.apache.pulsar.client.api.Message;
	using Schema = org.apache.pulsar.client.api.Schema;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test of <seealso cref="Message"/> methods.
	/// </summary>
	public class MessageTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageImplReplicatedInfo()
		public virtual void testMessageImplReplicatedInfo()
		{
			string from = "ClusterNameOfReplicatedFrom";
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setReplicatedFrom(from);
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
			Message<sbyte[]> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertTrue(msg.Replicated);
			assertEquals(msg.ReplicatedFrom, from);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageImplNoReplicatedInfo()
		public virtual void testMessageImplNoReplicatedInfo()
		{
			MessageMetadata.Builder builder = MessageMetadata.newBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
			Message<sbyte[]> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertFalse(msg.Replicated);
			assertTrue(msg.ReplicatedFrom.Empty);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTopicMessageImplReplicatedInfo()
		public virtual void testTopicMessageImplReplicatedInfo()
		{
			string from = "ClusterNameOfReplicatedFromForTopicMessage";
			string topicName = "myTopic";
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setReplicatedFrom(from);
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
			MessageImpl<sbyte[]> msg = MessageImpl.create(builder, payload, Schema.BYTES);
			msg.setMessageId(new MessageIdImpl(-1, -1, -1));
			TopicMessageImpl<sbyte[]> topicMessage = new TopicMessageImpl<sbyte[]>(topicName, topicName, msg);

			assertTrue(topicMessage.Replicated);
			assertEquals(msg.ReplicatedFrom, from);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTopicMessageImplNoReplicatedInfo()
		public virtual void testTopicMessageImplNoReplicatedInfo()
		{
			string topicName = "myTopic";
			MessageMetadata.Builder builder = MessageMetadata.newBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
			MessageImpl<sbyte[]> msg = MessageImpl.create(builder, payload, Schema.BYTES);
			msg.setMessageId(new MessageIdImpl(-1, -1, -1));
			TopicMessageImpl<sbyte[]> topicMessage = new TopicMessageImpl<sbyte[]>(topicName, topicName, msg);

			assertFalse(topicMessage.Replicated);
			assertTrue(topicMessage.ReplicatedFrom.Length == 0);
		}
	}

}