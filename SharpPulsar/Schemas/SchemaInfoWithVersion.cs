using SharpPulsar.Interfaces.ISchema;

namespace SharpPulsar.Schemas
{
    public class SchemaInfoWithVersion
    {
		public long Version { get; set; }

		private ISchemaInfo SchemaInfo { get; set; }

		public override string ToString()
		{
			return DefaultImplementation.JsonifySchemaInfoWithVersion(this);
		}
	}
}
