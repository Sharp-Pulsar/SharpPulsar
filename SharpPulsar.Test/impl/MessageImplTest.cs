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

using SharpPulsar.Impl;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using Xunit;

namespace SharpPulsar.Test.Impl
{

	/// <summary>
	/// Unit test of <seealso cref="MessageImpl{T}"/>.
	/// </summary>
	public class MessageImplTest
	{
		[Fact]
		public void TestGetSequenceIdNotAssociated()
		{
			var builder = new MessageMetadata();
            var payload = new byte[0];
			var msg = Message.Create(builder, payload, SchemaFields.Bytes, "test");

			Assert.Equal(msg.SequenceId, 0);
		}
		[Fact]
		public void TestGetSequenceIdAssociated()
		{
			var builder = new MessageMetadata();
            builder.SequenceId = (1234);

            var payload = new byte[0];
            var msg = Message.Create(builder, payload, SchemaFields.Bytes, "tests");

			Assert.Equal(1234, msg.SequenceId);
		}
		[Fact]
		public void TestGetProducerNameNotAssigned()
		{
			var builder = new MessageMetadata();

            var payload = new byte[0];
            var msg = Message.Create(builder, payload, SchemaFields.Bytes, "tests");

			Assert.Null(msg.ProducerName);
		}
		[Fact]
		public void TestGetProducerNameAssigned()
		{
            var builder = new MessageMetadata {ProducerName = "test-producer"};


            var payload = new byte[0];
            var msg = Message.Create(builder, payload, SchemaFields.Bytes, "Tests");

			Assert.Equal("test-producer", msg.ProducerName);
		}

		

	}

}