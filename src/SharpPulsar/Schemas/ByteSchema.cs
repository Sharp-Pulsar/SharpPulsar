﻿using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Schemas
{
    /// <summary>
    /// A schema for 'Byte'.
    /// </summary>
    public class ByteSchema : AbstractSchema<byte>
	{

		private static readonly ByteSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static ByteSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT8",
				Type = SchemaType.INT8,
				Schema = new byte[0]
			};
			_schemaInfo = info;
			_instance = new ByteSchema();
		}

		public static ByteSchema Of()
		{
			return _instance;
		}

		public override void Validate(byte[] message)
		{
			if (message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by ByteSchema is not 1");
			}
		}

		public override byte[] Encode(byte message)
		{
			return new byte[] { message };
		}

		public override byte Decode(byte[] bytes)
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
