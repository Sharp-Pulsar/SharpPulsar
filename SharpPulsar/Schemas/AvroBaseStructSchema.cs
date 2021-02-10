using System.Text;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;

namespace SharpPulsar.Schemas
{
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
			schema = SchemaUtils.ParseAvroSchema(Encoding.UTF8.GetString(schemaInfo.Schema.ToBytes()));
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
