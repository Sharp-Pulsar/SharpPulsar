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
namespace org.apache.pulsar.client.impl.schema.generic
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;


	using SchemaDefinition = api.schema.SchemaDefinition;
	using org.apache.pulsar.client.impl.schema;
	using TopicName = common.naming.TopicName;
	using SchemaInfo = common.schema.SchemaInfo;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test for <seealso cref="MultiVersionSchemaInfoProvider"/>.
	/// </summary>
	public class MultiVersionSchemaInfoProviderTest
	{

		private MultiVersionSchemaInfoProvider schemaProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			PulsarClientImpl client = mock(typeof(PulsarClientImpl));
			when(client.Lookup).thenReturn(mock(typeof(LookupService)));
			schemaProvider = new MultiVersionSchemaInfoProvider(TopicName.get("persistent://public/default/my-topic"), client);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSchema() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testGetSchema()
		{
			CompletableFuture<Optional<SchemaInfo>> completableFuture = new CompletableFuture<Optional<SchemaInfo>>();
			SchemaInfo schemaInfo = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils>().withPojo(typeof(SchemaTestUtils)).build()).SchemaInfo;
			completableFuture.complete(schemaInfo);
			when(schemaProvider.PulsarClient.Lookup.getSchema(any(typeof(TopicName)), any(typeof(sbyte[])))).thenReturn(completableFuture);
			SchemaInfo schemaInfoByVersion = schemaProvider.getSchemaByVersion(new sbyte[0]).get();
			assertEquals(schemaInfoByVersion, schemaInfo);
		}
	}

}