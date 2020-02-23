using FakeItEasy;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using Xunit;

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
namespace SharpPulsar.Test.Impl.schema.generic
{
    using Bar = SchemaTestUtils.Bar;
	using Foo = SchemaTestUtils.Foo;

/// <summary>
	/// Unit testing generic schemas.
	/// </summary>
	public class GenericSchemaImplTest
{
    private readonly ILogger Log = Utility.Log.Logger.CreateLogger<GenericSchemaImplTest>();
		
		public void TestGenericJsonSchema()
		{
			var encodeSchema = ISchema<Foo>.Json(typeof(Foo));
			var decodeSchema = GenericSchemaImpl.Of((SchemaInfo)encodeSchema.SchemaInfo);
			TestEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}
		
		public void TestAutoJsonSchema()
		{
            var config = new ClientConfigurationData {ServiceUrl = "pulsar://localhost:6650"};
            var client = new PulsarClientImpl(config);
			// configure the schema info provider
			var multiVersionSchemaInfoProvider = A.Fake<MultiVersionSchemaInfoProvider>(x=> x.WithArgumentsForConstructor(()=> new MultiVersionSchemaInfoProvider(new TopicName(), client)));
			
			// configure encode schema
			var encodeSchema = ISchema<Foo>.Json(typeof(Foo));

			// configure decode schema
			var decodeSchema = new AutoConsumeSchema();
			decodeSchema.ConfigureSchemaInfo("test-topic", "topic", (SchemaInfo)encodeSchema.SchemaInfo);
			decodeSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;

			TestEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}

		private void TestEncodeAndDecodeGenericRecord(ISchema<Foo> encodeSchema, ISchema<IGenericRecord> decodeSchema)
		{
			var numRecords = 10;
			for (var i = 0; i < numRecords; i++)
			{
				var foo = NewFoo(i);
				var data = encodeSchema.Encode(foo);

				Log.LogInformation("Decoding : {}", StringHelper.NewString(data));

                GenericJsonRecord record;
				if (decodeSchema is AutoConsumeSchema)
				{
					record = (GenericJsonRecord)decodeSchema.Decode(data, new sbyte[0]);
				}
				else
				{
					record = (GenericJsonRecord)decodeSchema.Decode(data);
				}
				VerifyFooRecord(record, i);
			}
		}

		private static Foo NewFoo(int i)
		{
            var foo = new Foo {Field1 = "field-1-" + i, Field2 = "field-2-" + i, Field3 = i};
            var bar = new Bar {Field1 = i % 2 == 0};
            foo.Field4 = bar;
			foo.FieldUnableNull = "fieldUnableNull-1-" + i;

			return foo;
		}

		private static void VerifyFooRecord(IGenericRecord record, int i)
		{
			var field1 = record.GetField("field1");
			Assert.Equal("field-1-" + i, field1);
			var field2 = record.GetField("field2");
            Assert.Equal("field-2-" + i, field2);
			var field3 = record.GetField("field3");
            Assert.Equal(i, field3);
			var field4 = record.GetField("field4");
			Assert.True(field4 is IGenericRecord);
			IGenericRecord field4Record = (GenericJsonRecord) field4;
            Assert.Equal(i % 2 == 0, field4Record.GetField("field1"));
			var fieldUnableNull = record.GetField("fieldUnableNull");
            Assert.Equal("fieldUnableNull-1-" + i, fieldUnableNull);
		}

	}

}