﻿/// <summary>
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
namespace Org.Apache.Pulsar.Client.Impl.Schema.Generic
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;


	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using LookupService = org.apache.pulsar.client.impl.LookupService;
	using PulsarClientImpl = org.apache.pulsar.client.impl.PulsarClientImpl;
	using AvroSchema = Org.Apache.Pulsar.Client.Impl.Schema.AvroSchema;
	using SchemaTestUtils = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test for <seealso cref="MultiVersionSchemaInfoProvider"/>.
	/// </summary>
	public class MultiVersionSchemaInfoProviderTest
	{

		private MultiVersionSchemaInfoProvider _schemaProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void Setup()
		{
			PulsarClientImpl Client = mock(typeof(PulsarClientImpl));
			when(Client.Lookup).thenReturn(mock(typeof(LookupService)));
			_schemaProvider = new MultiVersionSchemaInfoProvider(TopicName.get("persistent://public/default/my-topic"), Client);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSchema() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestGetSchema()
		{
			CompletableFuture<Optional<SchemaInfo>> CompletableFuture = new CompletableFuture<Optional<SchemaInfo>>();
			SchemaInfo SchemaInfo = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils>().withPojo(typeof(SchemaTestUtils)).build()).SchemaInfo;
			CompletableFuture.complete(SchemaInfo);
			when(_schemaProvider.PulsarClient.Lookup.getSchema(any(typeof(TopicName)), any(typeof(sbyte[])))).thenReturn(CompletableFuture);
			SchemaInfo SchemaInfoByVersion = _schemaProvider.getSchemaByVersion(new sbyte[0]).get();
			assertEquals(SchemaInfoByVersion, SchemaInfo);
		}
	}

}