﻿namespace SharpPulsar.Schemas
{
    public class SchemaInfoWithVersion
    {
		public long Version { get; set; }

        public override string ToString()
		{
			return DefaultImplementation.JsonifySchemaInfoWithVersion(this);
		}
	}
}
