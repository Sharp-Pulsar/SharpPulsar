using SharpPulsar.Common;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Schemas
{


    /// <summary>
    /// Information about the schema.
    /// </summary>
    public class SchemaInfo : ISchemaInfo
	{
		public string Name { get; set; }

		/// <summary>
		/// The schema data in AVRO JSON format.
		/// </summary>
		public byte[] Schema { get; set; }

		/// <summary>
		/// The type of schema (AVRO, JSON, PROTOBUF, etc..).
		/// </summary>
		public SchemaType Type { get; set; }

		/// <summary>
		/// Additional properties of the schema definition (implementation defined).
		/// </summary>
		public IDictionary<string, string> Properties {	get; set; } = new Dictionary<string, string>(); 
		public virtual string SchemaDefinition
		{
			get
			{
				if (null == Schema)
				{
					return "";
				}

				switch (Type.InnerEnumValue)
				{
					case SchemaType.InnerEnum.AVRO:
					case SchemaType.InnerEnum.JSON:
					case SchemaType.InnerEnum.PROTOBUF:
						return StringHelper.NewString(Schema, Encoding.UTF8.WebName);
					case SchemaType.InnerEnum.KeyValue:
					    KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(this);
					    return DefaultImplementation.JsonifyKeyValueSchemaInfo(schemaInfoKeyValue);
					default:
						return Convert.ToBase64String(Schema);
				}
			}
		}

        public override string ToString()
		{
			return DefaultImplementation.JsonifySchemaInfo(this);
		}

	}
}
