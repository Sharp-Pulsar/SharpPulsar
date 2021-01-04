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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using Data = lombok.Data;
	using Nullable = org.apache.avro.reflect.Nullable;
	using Schema = org.apache.pulsar.client.api.Schema;
	using org.apache.pulsar.client.api.schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Schema Builder Test.
	/// </summary>
	public class SchemaBuilderTest
	{

		private class AllOptionalFields
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<int> intField;
			internal int? IntField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<long> longField;
			internal long? LongField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private String stringField;
			internal string StringField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<bool> boolField;
			internal bool? BoolField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<float> floatField;
			internal float? FloatField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<double> doubleField;
			internal double? DoubleField;
		}

		private class AllPrimitiveFields
		{
			internal int IntField;
			internal long LongField;
			internal bool BoolField;
			internal float FloatField;
			internal double DoubleField;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class People
		private class People
		{
			internal People1 People1;
			internal People2 People2;
			internal string Name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class People1
		private class People1
		{
			internal int Age;
			internal int Height;
			internal string Name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class People2
		private class People2
		{
			internal int Age;
			internal int Height;
			internal string Name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllOptionalFieldsSchema()
		public virtual void TestAllOptionalFieldsSchema()
		{
			RecordSchemaBuilder RecordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllOptionalFields");
			RecordSchemaBuilder.field("intField").type(SchemaType.INT32).optional();
			RecordSchemaBuilder.field("longField").type(SchemaType.INT64).optional();
			RecordSchemaBuilder.field("stringField").type(SchemaType.STRING).optional();
			RecordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN).optional();
			RecordSchemaBuilder.field("floatField").type(SchemaType.FLOAT).optional();
			RecordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE).optional();
			SchemaInfo SchemaInfo = RecordSchemaBuilder.build(SchemaType.AVRO);

			Schema<AllOptionalFields> PojoSchema = Schema.AVRO(typeof(AllOptionalFields));
			SchemaInfo PojoSchemaInfo = PojoSchema.SchemaInfo;

			org.apache.avro.Schema AvroSchema = (new org.apache.avro.Schema.Parser()).parse(new string(SchemaInfo.Schema, UTF_8));
			org.apache.avro.Schema AvroPojoSchema = (new org.apache.avro.Schema.Parser()).parse(new string(PojoSchemaInfo.Schema, UTF_8));

			assertEquals(AvroPojoSchema, AvroSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllPrimitiveFieldsSchema()
		public virtual void TestAllPrimitiveFieldsSchema()
		{
			RecordSchemaBuilder RecordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			RecordSchemaBuilder.field("intField").type(SchemaType.INT32);
			RecordSchemaBuilder.field("longField").type(SchemaType.INT64);
			RecordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			RecordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			RecordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo SchemaInfo = RecordSchemaBuilder.build(SchemaType.AVRO);

			Schema<AllPrimitiveFields> PojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			SchemaInfo PojoSchemaInfo = PojoSchema.SchemaInfo;

			org.apache.avro.Schema AvroSchema = (new org.apache.avro.Schema.Parser()).parse(new string(SchemaInfo.Schema, UTF_8));
			org.apache.avro.Schema AvroPojoSchema = (new org.apache.avro.Schema.Parser()).parse(new string(PojoSchemaInfo.Schema, UTF_8));

			assertEquals(AvroPojoSchema, AvroSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderByFieldName()
		public virtual void TestGenericRecordBuilderByFieldName()
		{
			RecordSchemaBuilder RecordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			RecordSchemaBuilder.field("intField").type(SchemaType.INT32);
			RecordSchemaBuilder.field("longField").type(SchemaType.INT64);
			RecordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			RecordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			RecordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo SchemaInfo = RecordSchemaBuilder.build(SchemaType.AVRO);
			GenericSchema Schema = Schema.generic(SchemaInfo);
			GenericRecord Record = Schema.newRecordBuilder().set("intField", 32).set("longField", 1234L).set("boolField", true).set("floatField", 0.7f).set("doubleField", 1.34d).build();

			sbyte[] SerializedData = Schema.encode(Record);

			// create a POJO schema to deserialize the serialized data
			Schema<AllPrimitiveFields> PojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			AllPrimitiveFields Fields = PojoSchema.decode(SerializedData);

			assertEquals(32, Fields.IntField);
			assertEquals(1234L, Fields.LongField);
			assertEquals(true, Fields.BoolField);
			assertEquals(0.7f, Fields.FloatField);
			assertEquals(1.34d, Fields.DoubleField);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderByIndex()
		public virtual void TestGenericRecordBuilderByIndex()
		{
			RecordSchemaBuilder RecordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			RecordSchemaBuilder.field("intField").type(SchemaType.INT32);
			RecordSchemaBuilder.field("longField").type(SchemaType.INT64);
			RecordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			RecordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			RecordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo SchemaInfo = RecordSchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> Schema = Schema.generic(SchemaInfo);
			GenericRecord Record = Schema.newRecordBuilder().set(Schema.Fields.get(0), 32).set(Schema.Fields.get(1), 1234L).set(Schema.Fields.get(2), true).set(Schema.Fields.get(3), 0.7f).set(Schema.Fields.get(4), 1.34d).build();

			sbyte[] SerializedData = Schema.encode(Record);

			// create a POJO schema to deserialize the serialized data
			Schema<AllPrimitiveFields> PojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			AllPrimitiveFields Fields = PojoSchema.decode(SerializedData);

			assertEquals(32, Fields.IntField);
			assertEquals(1234L, Fields.LongField);
			assertEquals(true, Fields.BoolField);
			assertEquals(0.7f, Fields.FloatField);
			assertEquals(1.34d, Fields.DoubleField);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldname()
		public virtual void TestGenericRecordBuilderAvroByFieldname()
		{
			RecordSchemaBuilder People1SchemaBuilder = SchemaBuilder.record("People1");
			People1SchemaBuilder.field("age").type(SchemaType.INT32);
			People1SchemaBuilder.field("height").type(SchemaType.INT32);
			People1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo People1SchemaInfo = People1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema People1Schema = Schema.generic(People1SchemaInfo);


			GenericRecordBuilder People1RecordBuilder = People1Schema.newRecordBuilder();
			People1RecordBuilder.set("age", 20);
			People1RecordBuilder.set("height", 180);
			People1RecordBuilder.set("name", "people1");
			GenericRecord People1GenericRecord = People1RecordBuilder.build();

			RecordSchemaBuilder People2SchemaBuilder = SchemaBuilder.record("People2");
			People2SchemaBuilder.field("age").type(SchemaType.INT32);
			People2SchemaBuilder.field("height").type(SchemaType.INT32);
			People2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo People2SchemaInfo = People2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema People2Schema = Schema.generic(People2SchemaInfo);

			GenericRecordBuilder People2RecordBuilder = People2Schema.newRecordBuilder();
			People2RecordBuilder.set("age", 20);
			People2RecordBuilder.set("height", 180);
			People2RecordBuilder.set("name", "people2");
			GenericRecord People2GenericRecord = People2RecordBuilder.build();

			RecordSchemaBuilder PeopleSchemaBuilder = SchemaBuilder.record("People");
			PeopleSchemaBuilder.field("people1", People1Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("people2", People2Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo SchemaInfo = PeopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema PeopleSchema = Schema.generic(SchemaInfo);
			GenericRecordBuilder PeopleRecordBuilder = PeopleSchema.newRecordBuilder();
			PeopleRecordBuilder.set("people1", People1GenericRecord);
			PeopleRecordBuilder.set("people2", People2GenericRecord);
			PeopleRecordBuilder.set("name", "people");
			GenericRecord PeopleRecord = PeopleRecordBuilder.build();

			sbyte[] PeopleEncode = PeopleSchema.encode(PeopleRecord);

			GenericRecord People = (GenericRecord) PeopleSchema.decode(PeopleEncode);

			assertEquals(People.Fields, PeopleRecord.Fields);
			assertEquals((People.getField("name")), PeopleRecord.getField("name"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("age"), People1GenericRecord.getField("age"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("heigth"), People1GenericRecord.getField("heigth"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("name"), People1GenericRecord.getField("name"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("age"), People2GenericRecord.getField("age"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("height"), People2GenericRecord.getField("height"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("name"), People2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldnamePojo()
		public virtual void TestGenericRecordBuilderAvroByFieldnamePojo()
		{
			RecordSchemaBuilder People1SchemaBuilder = SchemaBuilder.record("People1");
			People1SchemaBuilder.field("age").type(SchemaType.INT32);
			People1SchemaBuilder.field("height").type(SchemaType.INT32);
			People1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo People1SchemaInfo = People1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema People1Schema = Schema.generic(People1SchemaInfo);


			GenericRecordBuilder People1RecordBuilder = People1Schema.newRecordBuilder();
			People1RecordBuilder.set("age", 20);
			People1RecordBuilder.set("height", 180);
			People1RecordBuilder.set("name", "people1");
			GenericRecord People1GenericRecord = People1RecordBuilder.build();

			RecordSchemaBuilder People2SchemaBuilder = SchemaBuilder.record("People2");
			People2SchemaBuilder.field("age").type(SchemaType.INT32);
			People2SchemaBuilder.field("height").type(SchemaType.INT32);
			People2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo People2SchemaInfo = People2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema People2Schema = Schema.generic(People2SchemaInfo);

			GenericRecordBuilder People2RecordBuilder = People2Schema.newRecordBuilder();
			People2RecordBuilder.set("age", 20);
			People2RecordBuilder.set("height", 180);
			People2RecordBuilder.set("name", "people2");
			GenericRecord People2GenericRecord = People2RecordBuilder.build();

			RecordSchemaBuilder PeopleSchemaBuilder = SchemaBuilder.record("People");
			PeopleSchemaBuilder.field("people1", People1Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("people2", People2Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo SchemaInfo = PeopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema PeopleSchema = Schema.generic(SchemaInfo);
			GenericRecordBuilder PeopleRecordBuilder = PeopleSchema.newRecordBuilder();
			PeopleRecordBuilder.set("people1", People1GenericRecord);
			PeopleRecordBuilder.set("people2", People2GenericRecord);
			PeopleRecordBuilder.set("name", "people");
			GenericRecord PeopleRecord = PeopleRecordBuilder.build();

			sbyte[] PeopleEncode = PeopleSchema.encode(PeopleRecord);

			Schema<People> PeopleDecodeSchema = Schema.AVRO(SchemaDefinition.builder<People>().withPojo(typeof(People)).withAlwaysAllowNull(false).build());
			People People = PeopleDecodeSchema.decode(PeopleEncode);

			assertEquals(People.Name, PeopleRecord.getField("name"));
			assertEquals(People.People1.age, People1GenericRecord.getField("age"));
			assertEquals(People.People1.height, People1GenericRecord.getField("height"));
			assertEquals(People.People1.name, People1GenericRecord.getField("name"));
			assertEquals(People.People2.age, People2GenericRecord.getField("age"));
			assertEquals(People.People2.height, People2GenericRecord.getField("height"));
			assertEquals(People.People2.name, People2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldIndex()
		public virtual void TestGenericRecordBuilderAvroByFieldIndex()
		{
			RecordSchemaBuilder People1SchemaBuilder = SchemaBuilder.record("People1");
			People1SchemaBuilder.field("age").type(SchemaType.INT32);
			People1SchemaBuilder.field("height").type(SchemaType.INT32);
			People1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo People1SchemaInfo = People1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> People1Schema = Schema.generic(People1SchemaInfo);


			GenericRecordBuilder People1RecordBuilder = People1Schema.newRecordBuilder();
			People1RecordBuilder.set(People1Schema.Fields.get(0), 20);
			People1RecordBuilder.set(People1Schema.Fields.get(1), 180);
			People1RecordBuilder.set(People1Schema.Fields.get(2), "people1");
			GenericRecord People1GenericRecord = People1RecordBuilder.build();

			RecordSchemaBuilder People2SchemaBuilder = SchemaBuilder.record("People2");
			People2SchemaBuilder.field("age").type(SchemaType.INT32);
			People2SchemaBuilder.field("height").type(SchemaType.INT32);
			People2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo People2SchemaInfo = People2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> People2Schema = Schema.generic(People2SchemaInfo);

			GenericRecordBuilder People2RecordBuilder = People2Schema.newRecordBuilder();
			People2RecordBuilder.set(People2Schema.Fields.get(0), 20);
			People2RecordBuilder.set(People2Schema.Fields.get(1), 180);
			People2RecordBuilder.set(People2Schema.Fields.get(2), "people2");
			GenericRecord People2GenericRecord = People2RecordBuilder.build();

			RecordSchemaBuilder PeopleSchemaBuilder = SchemaBuilder.record("People");
			PeopleSchemaBuilder.field("people1", People1Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("people2", People2Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo SchemaInfo = PeopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema<GenericRecord> PeopleSchema = Schema.generic(SchemaInfo);
			GenericRecordBuilder PeopleRecordBuilder = PeopleSchema.newRecordBuilder();
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(0), People1GenericRecord);
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(1), People2GenericRecord);
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(2), "people");
			GenericRecord PeopleRecord = PeopleRecordBuilder.build();

			sbyte[] PeopleEncode = PeopleSchema.encode(PeopleRecord);

			GenericRecord People = (GenericRecord) PeopleSchema.decode(PeopleEncode);

			assertEquals(People.Fields, PeopleRecord.Fields);
			assertEquals((People.getField("name")), PeopleRecord.getField("name"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("age"), People1GenericRecord.getField("age"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("heigth"), People1GenericRecord.getField("heigth"));
			assertEquals(((GenericRecord)People.getField("people1")).getField("name"), People1GenericRecord.getField("name"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("age"), People2GenericRecord.getField("age"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("height"), People2GenericRecord.getField("height"));
			assertEquals(((GenericRecord)People.getField("people2")).getField("name"), People2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldIndexPojo()
		public virtual void TestGenericRecordBuilderAvroByFieldIndexPojo()
		{
			RecordSchemaBuilder People1SchemaBuilder = SchemaBuilder.record("People1");
			People1SchemaBuilder.field("age").type(SchemaType.INT32);
			People1SchemaBuilder.field("height").type(SchemaType.INT32);
			People1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo People1SchemaInfo = People1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> People1Schema = Schema.generic(People1SchemaInfo);


			GenericRecordBuilder People1RecordBuilder = People1Schema.newRecordBuilder();
			People1RecordBuilder.set(People1Schema.Fields.get(0), 20);
			People1RecordBuilder.set(People1Schema.Fields.get(1), 180);
			People1RecordBuilder.set(People1Schema.Fields.get(2), "people1");
			GenericRecord People1GenericRecord = People1RecordBuilder.build();

			RecordSchemaBuilder People2SchemaBuilder = SchemaBuilder.record("People2");
			People2SchemaBuilder.field("age").type(SchemaType.INT32);
			People2SchemaBuilder.field("height").type(SchemaType.INT32);
			People2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo People2SchemaInfo = People2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> People2Schema = Schema.generic(People2SchemaInfo);

			GenericRecordBuilder People2RecordBuilder = People2Schema.newRecordBuilder();
			People2RecordBuilder.set(People2Schema.Fields.get(0), 20);
			People2RecordBuilder.set(People2Schema.Fields.get(1), 180);
			People2RecordBuilder.set(People2Schema.Fields.get(2), "people2");
			GenericRecord People2GenericRecord = People2RecordBuilder.build();

			RecordSchemaBuilder PeopleSchemaBuilder = SchemaBuilder.record("People");
			PeopleSchemaBuilder.field("people1", People1Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("people2", People2Schema).type(SchemaType.AVRO);
			PeopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo SchemaInfo = PeopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema<GenericRecord> PeopleSchema = Schema.generic(SchemaInfo);
			GenericRecordBuilder PeopleRecordBuilder = PeopleSchema.newRecordBuilder();
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(0), People1GenericRecord);
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(1), People2GenericRecord);
			PeopleRecordBuilder.set(PeopleSchema.Fields.get(2), "people");
			GenericRecord PeopleRecord = PeopleRecordBuilder.build();

			sbyte[] PeopleEncode = PeopleSchema.encode(PeopleRecord);

			Schema<People> PeopleDecodeSchema = Schema.AVRO(SchemaDefinition.builder<People>().withPojo(typeof(People)).withAlwaysAllowNull(false).build());
			People People = PeopleDecodeSchema.decode(PeopleEncode);

			assertEquals(People.Name, PeopleRecord.getField("name"));
			assertEquals(People.People1.age, People1GenericRecord.getField("age"));
			assertEquals(People.People1.height, People1GenericRecord.getField("height"));
			assertEquals(People.People1.name, People1GenericRecord.getField("name"));
			assertEquals(People.People2.age, People2GenericRecord.getField("age"));
			assertEquals(People.People2.height, People2GenericRecord.getField("height"));
			assertEquals(People.People2.name, People2GenericRecord.getField("name"));
		}
	}

}