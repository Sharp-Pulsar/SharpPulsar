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
	/// Unit test of <seealso cref="MessageImpl{T}"/>.
	/// </summary>
	public class MessageImplTest
	{
		[Fact]
		public void TestGetSequenceIdNotAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder();
            var payload = Unpooled.WrappedBuffer(new byte[0]);
			var msg = MessageImpl<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.Equal(msg.SequenceId, -1);
		}
		[Fact]
		public void TestGetSequenceIdAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().SetSequenceId(1234);

            var payload = Unpooled.WrappedBuffer(new byte[0]);
            var msg = MessageImpl<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.Equal(1234, msg.SequenceId);
		}
		[Fact]
		public void TestGetProducerNameNotAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder();

            var payload = Unpooled.WrappedBuffer(new byte[0]);
            var msg = MessageImpl<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.Null(msg.ProducerName);
		}
		[Fact]
		public void TestGetProducerNameAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().SetProducerName("test-producer");

            var payload = Unpooled.WrappedBuffer(new byte[0]);
            var msg = MessageImpl<sbyte[]>.Create(builder, payload, SchemaFields.Bytes);

			Assert.Equal("test-producer", msg.ProducerName);
		}

		

	}

}