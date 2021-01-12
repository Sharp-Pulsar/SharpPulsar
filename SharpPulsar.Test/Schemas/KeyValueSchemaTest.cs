using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static SharpPulsar.Test.Schema.SchemaTestUtils;

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
namespace SharpPulsar.Test.Schema
{
    public class KeyValueSchemaTest
	{
		[Fact]
		public void TestAllowNullAvroSchemaCreate()
		{
			AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			ISchema<KeyValue<Foo, Bar>> keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
			ISchema<KeyValue<Foo, Bar>> keyValueSchema2 = ISchema<object>.KeyValue<Foo, Bar>(typeof(Foo), typeof(Bar), SchemaType.AVRO);

			Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
			Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);

			Assert.Equal(((KeyValueSchema<Foo, Bar>) keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			Assert.Equal(((KeyValueSchema<Foo, Bar>) keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
			Assert.Equal(((KeyValueSchema<Foo, Bar>) keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			Assert.Equal(((KeyValueSchema<Foo, Bar>) keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

			var schemaInfo1 = Encoding.UTF8.GetString((byte[])(object)keyValueSchema1.SchemaInfo.Schema);
			var schemaInfo2 = Encoding.UTF8.GetString((byte[])(object)keyValueSchema2.SchemaInfo.Schema);
			Assert.Equal(schemaInfo1, schemaInfo2);
		}
		[Fact]
		public void TestFillParametersToSchemainfo()
		{
			AvroSchema<Foo> FooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			AvroSchema<Bar> BarSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			FooSchema.SchemaInfo.Name = "foo";
			FooSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> KeyProperties = new Dictionary<string, string>();
			KeyProperties["foo.key1"] = "value";
			KeyProperties["foo.key2"] = "value";
			FooSchema.SchemaInfo.Properties = KeyProperties;
			BarSchema.SchemaInfo.Name = "bar";
			BarSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> ValueProperties = new Dictionary<string, string>(); 
			ValueProperties["bar.key"] = "key";
			BarSchema.SchemaInfo.Properties = ValueProperties;
			ISchema<KeyValue<Foo, Bar>> KeyValueSchema1 = ISchema<object>.KeyValue(FooSchema, BarSchema);

			Assert.Equal("foo", KeyValueSchema1.SchemaInfo.Properties["key.schema.name"]);
			Assert.Equal(KeyValueSchema1.SchemaInfo.Properties["key.schema.type"], SchemaType.AVRO.ToString());
			Assert.Equal("{\"foo.key1\":\"value\",\"foo.key2\":\"value\"}", KeyValueSchema1.SchemaInfo.Properties["key.schema.properties"]);
			Assert.Equal("bar", KeyValueSchema1.SchemaInfo.Properties["value.schema.name"]);
			Assert.Equal(KeyValueSchema1.SchemaInfo.Properties["value.schema.type"], SchemaType.AVRO.ToString());
			Assert.Equal("{\"bar.key\":\"key\"}", KeyValueSchema1.SchemaInfo.Properties["value.schema.properties"]);
		}

		[Fact]
		public void TestAllowNullJsonSchemaCreate()
		{
			JSONSchema<Foo> fooSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			JSONSchema<Bar> barSchema = JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			ISchema<KeyValue<Foo, Bar>> keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
			ISchema<KeyValue<Foo, Bar>> keyValueSchema2 = ISchema<object>.KeyValue<Foo, Bar>(typeof(Foo), typeof(Bar), SchemaType.JSON);
			ISchema<KeyValue<Foo, Bar>> keyValueSchema3 = ISchema<object>.KeyValue<Foo, Bar>(typeof(Foo), typeof(Bar));

			Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
			Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);
			Assert.Equal(keyValueSchema3.SchemaInfo.Type, SchemaType.KeyValue);

			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

			var schemaInfo1 = Encoding.UTF8.GetString((byte[])(object)keyValueSchema1.SchemaInfo.Schema);
			var schemaInfo2 = Encoding.UTF8.GetString((byte[])(object)keyValueSchema2.SchemaInfo.Schema);
			var schemaInfo3 = Encoding.UTF8.GetString((byte[])(object)keyValueSchema3.SchemaInfo.Schema);
			Assert.Equal(schemaInfo1, schemaInfo2);
			Assert.Equal(schemaInfo1, schemaInfo3);
		}
		[Fact]
		public void TestDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			AvroSchema<Bar> BarSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			ISchema<KeyValue<Foo, Bar>> KeyValueSchema = ISchema<object>.KeyValue(FooSchema, BarSchema);

			Bar Bar = new Bar();
			Bar.Field1 = true;

            Foo Foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3,
                Field4 = Bar,
                Color = Color.RED
            };

            // Check kv.encoding.type default not set value
            sbyte[] EncodeBytes = KeyValueSchema.Encode(new KeyValue<Foo, Bar>(Foo, Bar));
			Assert.True(EncodeBytes.Length > 0);

			KeyValue<Foo, Bar> KeyValue = KeyValueSchema.Decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;

			Assert.Equal(Foo, FooBack);
			Assert.Equal(Bar, BarBack);
		}

		[Fact]
		public void TestInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
		{

			AvroSchema<Foo> FooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			AvroSchema<Bar> BarSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			ISchema<KeyValue<Foo, Bar>> KeyValueSchema = ISchema<KeyValue<Foo, Bar>>.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);


            Bar Bar = new Bar
            {
                Field1 = true
            };

            Foo Foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3,
                Field4 = Bar,
                Color = Color.RED
            };

            // Check kv.encoding.type INLINE
            sbyte[] EncodeBytes = KeyValueSchema.Encode(new KeyValue<Foo,Bar> (Foo, Bar));
			Assert.True(EncodeBytes.Length > 0);
			KeyValue<Foo, Bar> KeyValue = (KeyValue<Foo, Bar>) KeyValueSchema.Decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;
			Assert.Equal(Foo, FooBack);
			Assert.Equal(Bar, BarBack);

		}

		[Fact]
		public void TestSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
			AvroSchema<Bar> BarSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

			ISchema<KeyValue<Foo, Bar>> KeyValueSchema = ISchema<KeyValue<Foo, Bar>>.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);


            Bar Bar = new Bar
            {
                Field1 = true
            };

            Foo Foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = 3,
                Field4 = Bar,
                Color = Color.RED
            };

            // Check kv.encoding.type SEPARATED
            sbyte[] EncodeBytes = KeyValueSchema.Encode(new KeyValue<Foo, Bar>(Foo, Bar));
			Assert.True(EncodeBytes.Length > 0);
			try
			{
				KeyValueSchema.Decode(EncodeBytes);
			}
			catch (SchemaSerializationException E)
			{
				Assert.Contains("This method cannot be used under this SEPARATED encoding type", E.Message);
			}
			KeyValue<Foo, Bar> KeyValue = ((KeyValueSchema<Foo, Bar>)KeyValueSchema).Decode(FooSchema.Encode(Foo), EncodeBytes, null);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;
			Assert.Equal(Foo, FooBack);
			Assert.Equal(Bar, BarBack);
		}

	}

}