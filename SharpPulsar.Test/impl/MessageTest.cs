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

using DotNetty.Buffers;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using Xunit;

namespace SharpPulsar.Test.Impl
{

/// <summary>
	/// Unit test of <seealso cref="Message"/> methods.
	/// </summary>
	public class MessageTest
	{
		[Fact]
		public void TestMessageImplReplicatedInfo()
		{
			var @from = "ClusterNameOfReplicatedFrom";
			var builder = MessageMetadata.NewBuilder().SetReplicatedFrom(@from);
			var payload = Unpooled.WrappedBuffer(new byte[0]);
			var msg = Message<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.True(msg.Replicated);
			Assert.Equal(@from, msg.ReplicatedFrom);
		}
		[Fact]
		public  void TestMessageImplNoReplicatedInfo()
		{
			var builder = MessageMetadata.NewBuilder();
            var payload = Unpooled.WrappedBuffer(new byte[0]);
			var msg = Message<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.False(msg.Replicated);
			Assert.True(msg.ReplicatedFrom.Length == 0);
		}
		[Fact]
		public void TestTopicMessageImplReplicatedInfo()
		{
			var @from = "ClusterNameOfReplicatedFromForTopicMessage";
			var topicName = "myTopic";
			var builder = MessageMetadata.NewBuilder().SetReplicatedFrom(@from);
            var payload = Unpooled.WrappedBuffer(new byte[0]);
			var msg = Message<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);
			msg.SetMessageId(new MessageId(-1, -1, -1));
			var topicMessage = new TopicMessageImpl<sbyte[]>(topicName, topicName, msg);

			Assert.True(topicMessage.Replicated);
			Assert.Equal(@from, msg.ReplicatedFrom);
		}
		[Fact]
		public void TestTopicMessageImplNoReplicatedInfo()
		{
			var topicName = "myTopic";
			var builder = MessageMetadata.NewBuilder();
            var payload = Unpooled.WrappedBuffer(new byte[0]);
			var msg = Message<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);
			msg.SetMessageId(new MessageId(-1, -1, -1));
			var topicMessage = new TopicMessageImpl<sbyte[]>(topicName, topicName, msg);

			Assert.False(topicMessage.Replicated);
			Assert.True(topicMessage.ReplicatedFrom.Length == 0);
		}
	}

}