using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System;


namespace SharpPulsar.Schemas
{
	/// <summary>
	/// Encodes DateTimeOffset.ToUnixTimeMilliseconds
	/// Decodes DateTimeOffset.FromUnixTimeMilliseconds
	/// </summary>
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
				Schema = new byte[0]
			};
			_schemaInfo = info;
			_instance = new TimestampSchema();
		}

		public static TimestampSchema Of()
		{
			return _instance;
		}

		public override byte[] Encode(DateTimeOffset message)
		{
			long time = message.ToUnixTimeMilliseconds().LongToBigEndian();
			return BitConverter.GetBytes(time);
		}

		public override DateTimeOffset Decode(byte[] bytes)
		{
			return DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(bytes, 0).LongFromBigEndian());
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
