using SharpPulsar.API;
using SharpPulsar.API.Internal;

namespace SharpPulsar.Schemas
{
    public class SchemaInfoWithVersion: ISchemaInfoWithVersion
    {
		public long Version { get; set; }

        public override string ToString()
		{
			return DefaultImplementation.DefaultImpl.JsonifySchemaInfoWithVersion(this);
		}
	}
}
