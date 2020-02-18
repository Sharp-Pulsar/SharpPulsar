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

using System.Threading.Tasks;
using DotNetty.Common.Concurrency;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using Xunit;

namespace SharpPulsar.Test.Impl.schema.generic
{
/// <summary>
	/// Unit test for <seealso cref="MultiVersionSchemaInfoProvider"/>.
	/// </summary>
	public class MultiVersionSchemaInfoProviderTest
	{

		private MultiVersionSchemaInfoProvider _schemaProvider;

		public void Setup()
		{
			PulsarClientImpl client = new Moq.Mock<PulsarClientImpl>().Object;
			When(client.Lookup).thenReturn(mock(typeof(LookupService)));
			_schemaProvider = new MultiVersionSchemaInfoProvider(TopicName.Get("persistent://public/default/my-topic"), client);
		}
		[Fact]
		public void TestGetSchema()
		{
			var task = new TaskCompletionSource<SchemaInfo>();
			var schemaInfo = (SchemaInfo)JsonSchema<SchemaTestUtils>.Of(ISchemaDefinition<SchemaTestUtils>.Builder().WithPojo(new SchemaTestUtils()).Build()).SchemaInfo;
			task.SetResult(schemaInfo);
			when(_schemaProvider.PulsarClient.Lookup.GetSchema(any(typeof(TopicName)), any(typeof(sbyte[])))).thenReturn(completableFuture);
			SchemaInfo schemaInfoByVersion = (SchemaInfo)_schemaProvider.GetSchemaByVersion(new sbyte[0]).Result;
			Assert.Equal(schemaInfoByVersion, schemaInfo);
		}
	}

}