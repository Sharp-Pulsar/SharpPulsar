using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Schemas;
using Xunit;
using SharpPulsar.Extension;
using System;
using System.Buffers;
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

	/// <summary>
	/// Unit test of <seealso cref="MessageImpl"/>.
	/// </summary>
	public class MessageTest
	{
		
		[Fact]
		public virtual void TestSetPropertiesKey()
		{
			var builder = new MessageMetadata();
			builder.Properties.Add(new KeyValue { Key = "key1", Value = "value1" });
			builder.Properties.Add(new KeyValue { Key = "key2", Value = "value2" });
			builder.Properties.Add(new KeyValue { Key = "key3", Value = "value3" });
			var payload = ReadOnlySequence<byte>.Empty;
			
			var msg = Message<byte[]>.Create(builder, payload, ISchema<object>.Bytes);
			Assert.Equal("value1", msg.GetProperty("key1"));
			Assert.Equal("value3", msg.GetProperty("key3"));
		}

		[Fact]
		public virtual void TestGetSequenceIdAssociated()
		{
            var builder = new MessageMetadata
            {
                SequenceId = 1234
            };

            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<object>.Bytes);

			Assert.Equal(1234, msg.SequenceId);
		}
		[Fact]
		public virtual void TestGetProducerNameNotAssigned()
		{
			var builder = new MessageMetadata();
            var payload = ReadOnlySequence<byte>.Empty;

            var msg = Message<byte[]>.Create(builder, payload, ISchema<object>.Bytes);

			Assert.Null(msg.ProducerName);
		}
		[Fact]
		public virtual void TestGetProducerNameAssigned()
		{
			var builder = new MessageMetadata();
			builder.ProducerName = "test-producer";
            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<object>.Bytes);

			Assert.Equal("test-producer", msg.ProducerName);
		}

		[Fact]
		public virtual void TestDefaultGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithPojo(typeof(SchemaTestUtils.Foo)).Build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema<SchemaTestUtils.Bar>.Of(ISchemaDefinition<SchemaTestUtils.Bar>.Builder().WithPojo(typeof(SchemaTestUtils.Bar)).Build());

			ISchema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema);
            SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3
            };
            SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar
            {
                Field1 = true
            };

            // // Check kv.encoding.type default, not set value
            var encodeBytes = keyValueSchema.Encode(new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar));
			var builder = new MessageMetadata();
			builder.ProducerName = "default";

			var msg = Message<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>>.Create(builder, new ReadOnlySequence<byte>(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			Assert.Equal(keyValue.Key, foo);
			Assert.Equal(keyValue.Value, bar);
			Assert.False(builder.ShouldSerializePartitionKey());
		}

		[Fact]
		public virtual void TestInlineGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithPojo(typeof(SchemaTestUtils.Foo)).Build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema<SchemaTestUtils.Bar>.Of(ISchemaDefinition<SchemaTestUtils.Bar>.Builder().WithPojo(typeof(SchemaTestUtils.Bar)).Build());

			var keyValueSchema = ISchema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);
            SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3
            };
            SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar
            {
                Field1 = true
            };

            // Check kv.encoding.type INLINE
            var encodeBytes = keyValueSchema.Encode(new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar));
			var builder = new MessageMetadata();
			builder.ProducerName = "inline";
			var msg = Message<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>>.Create(builder, new ReadOnlySequence<byte>(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			Assert.Equal(keyValue.Key, foo);
			Assert.Equal(keyValue.Value, bar);
			Assert.False(builder.ShouldSerializePartitionKey());
		}
		[Fact]
		public virtual void TestSeparatedGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithPojo(typeof(SchemaTestUtils.Foo)).Build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema<SchemaTestUtils.Bar>.Of(ISchemaDefinition<SchemaTestUtils.Bar>.Builder().WithPojo(typeof(SchemaTestUtils.Bar)).Build());

			var keyValueSchema = ISchema<int>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
            SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3
            };
            SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar
            {
                Field1 = true
            };

            // Check kv.encoding.type SPRAERATE
            var encodeBytes = keyValueSchema.Encode(new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar));
			var builder = new MessageMetadata();
			builder.ProducerName = "separated";
			builder.PartitionKey = Convert.ToBase64String(fooSchema.Encode(foo));
			builder.PartitionKeyB64Encoded = true;
			var msg = Message<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>>.Create(builder, new ReadOnlySequence<byte>(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			Assert.Equal(keyValue.Key, foo);
			Assert.Equal(keyValue.Value, bar);
			Assert.True(builder.ShouldSerializePartitionKey());
		}
		[Fact]
		public virtual void TestMessageImplReplicatedInfo()
		{
			string from = "ClusterNameOfReplicatedFrom";
			var builder = new MessageMetadata();
			builder.ReplicatedFrom = from;
            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<int>.Bytes);

			Assert.True(msg.Replicated);
			Assert.Equal(msg.ReplicatedFrom, from);
		}
		[Fact]
		public virtual void TestMessageImplNoReplicatedInfo()
		{
			var builder = new MessageMetadata();
            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<int>.Bytes);

			Assert.False(msg.Replicated);
			Assert.True(msg.ReplicatedFrom.Length == 0);
		}
		[Fact]
		public virtual void TestTopicMessageImplReplicatedInfo()
		{
			string from = "ClusterNameOfReplicatedFromForTopicMessage";
			string topicName = "myTopic";
			var builder = new MessageMetadata();
			builder.ReplicatedFrom = from;
            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<int>.Bytes);
			msg.SetMessageId(new MessageId(-1, -1, -1));
			var topicMessage = new TopicMessage<byte[]>(topicName, topicName, msg);

			Assert.True(topicMessage.Replicated);
			Assert.Equal(msg.ReplicatedFrom, from);
		}

		[Fact]
		public virtual void TestTopicMessageImplNoReplicatedInfo()
		{
			string topicName = "myTopic";
			var builder = new MessageMetadata();
            var payload = ReadOnlySequence<byte>.Empty;
            var msg = Message<byte[]>.Create(builder, payload, ISchema<int>.Bytes);
			msg.SetMessageId(new MessageId(-1, -1, -1));
			var topicMessage = new TopicMessage<byte[]>(topicName, topicName, msg);

			Assert.False(topicMessage.Replicated);
			Assert.True(topicMessage.ReplicatedFrom.Length == 0);
		}
	}

}