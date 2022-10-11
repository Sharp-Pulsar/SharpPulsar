using Avro;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Field = SharpPulsar.Interfaces.Schema.Field;

namespace SharpPulsar.Schemas.Generic
{
    public abstract class GenericSchema : AvroBaseStructSchema<IGenericRecord>, IGenericSchema<IGenericRecord>
	{
		public abstract IGenericRecordBuilder NewRecordBuilder();
		public abstract IGenericSchema<IGenericRecord> Generic(ISchemaInfo schemaInfo);
		public abstract ISchema<object> GetSchema(ISchemaInfo schemaInfo);
		public abstract ISchema<byte[]> AutoProduceBytes<T>(ISchema<T> schema);
		public abstract ISchema<byte[]> AutoProduceBytes();
		public abstract ISchema<IGenericRecord> AutoConsume();
		public abstract ISchema<IGenericRecord> Auto();
		public abstract ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value, KeyValueEncodingType keyValueEncodingType);
		public abstract ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value);
		public abstract ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value);
		public abstract ISchema<KeyValue<byte[], byte[]>> KvBytes();
		public abstract ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value, SchemaType type);
		public abstract ISchema<T> Json<T>(ISchemaDefinition<T> schemaDefinition);
		public abstract ISchema<T> Json<T>(Type pojo);
		public abstract ISchema<T> AVRO<T>(ISchemaDefinition<T> schemaDefinition);
		public abstract ISchema<T> AVRO<T>(Type pojo);
		public abstract void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo);
		public abstract bool RequireFetchingSchemaInfo();
		public override abstract bool SupportSchemaVersioning();
		public override abstract void Validate(byte[] message);

		private readonly IList<Field> _fields;

		protected internal GenericSchema(ISchemaInfo schemaInfo) : base(schemaInfo)
		{

			_fields = ((RecordSchema)schema).Fields.Select(f => new Field(f.Name, f.Pos)).ToList();
		}

		public virtual IList<Field> Fields
		{
			get
			{
				return _fields;
			}
		}

		/// <summary>
		/// Create a generic schema out of a <tt>SchemaInfo</tt>.
		///  warning : we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema 、GenericAvroSchema ) </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		public static GenericSchema Of(ISchemaInfo schemaInfo)
		{
			return Of(schemaInfo, true);
		}

		/// <summary>
		/// warning :
		/// we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema 、GenericAvroSchema ) </summary>
		/// <param name="schemaInfo"> <seealso cref="SchemaInfo"/> </param>
		/// <param name="useProvidedSchemaAsReaderSchema"> <seealso cref="Boolean"/> </param>
		/// <returns> generic schema implementation </returns>
		public static GenericSchema Of(ISchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema)
		{
			switch (schemaInfo.Type.Name)
			{
				case "AVRO":
				case "JSON":
					return new GenericAvroSchema(schemaInfo);
				default:
					throw new NotSupportedException("Generic schema is not supported on schema type " + schemaInfo.Type + "'");
			}
		}
	}


}
