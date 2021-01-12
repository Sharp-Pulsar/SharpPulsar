using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Schema
{
	/// <summary>
	/// A schema for 'Byte'.
	/// </summary>
	public class ByteSchema : AbstractSchema<sbyte>
	{

		private static readonly ByteSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static ByteSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT8",
				Type = SchemaType.INT8,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new ByteSchema();
		}

		public static ByteSchema Of()
		{
			return _instance;
		}

		public override void Validate(sbyte[] message)
		{
			if (message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by ByteSchema is not 1");
			}
		}

		public override sbyte[] Encode(sbyte message)
		{
			return new sbyte[] { message };
		}

		public override sbyte Decode(sbyte[] bytes)
		{
			Validate(bytes);
			return bytes[0];
		}


		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}
}
