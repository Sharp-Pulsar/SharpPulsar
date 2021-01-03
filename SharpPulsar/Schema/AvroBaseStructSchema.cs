using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Interfaces.Interceptor;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Schema
{

	//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	//	import static org.apache.pulsar.client.impl.schema.util.SchemaUtil.parseAvroSchema;

	/// <summary>
	/// This is a base schema implementation for Avro Based `Struct` types.
	/// A struct type is used for presenting records (objects) which
	/// have multiple fields.
	/// 
	/// <para>Currently Pulsar supports 3 `Struct` types -
	/// <seealso cref="SchemaType.Avro"/>,
	/// <seealso cref="SchemaType.Json"/>,
	/// and <seealso cref="SchemaType.Protobuf"/>.
	/// </para>
	/// </summary>
	public abstract class AvroBaseStructSchema<T> : AbstractStructSchema<T>
	{

		protected internal readonly Avro.Schema schema;

		public AvroBaseStructSchema(ISchemaInfo schemaInfo) : base(schemaInfo)
		{
			this.schema = SchemaUtils.ParseAvroSchema(Encoding.UTF8.GetString((byte[])(object)schemaInfo.Schema));
		}


        public virtual Avro.Schema AvroSchema
		{
			get
			{
				return schema;
			}
		}
	}
}
