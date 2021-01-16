using SharpPulsar.Common.Schema;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Schemas
{
    public class TimestampSchema : AbstractSchema<DateTimeOffset>
    {
		private static readonly TimestampSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static TimestampSchema()
		{
			var info = new SchemaInfo
			{
				Name = "Timestamp",
				Type = SchemaType.TIMESTAMP,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new TimestampSchema();
		}

		public static TimestampSchema Of()
		{
			return _instance;
		}

		public override sbyte[] Encode(DateTimeOffset message)
		{
			long time = message.ToUnixTimeSeconds().LongToBigEndian();
			return BitConverter.GetBytes(time).ToSBytes();
		}

		public override DateTimeOffset Decode(sbyte[] bytes)
		{
			return DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(bytes.ToBytes(), 0).LongFromBigEndian());
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
