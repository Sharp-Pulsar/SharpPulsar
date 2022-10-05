namespace SharpPulsar.Trino.Trino
{
    internal class ClientStandardTypes
    {
        public const string BIGINT = "bigint";
        public const string INTEGER = "integer";
        public const string SMALLINT = "smallint";
        public const string TINYINT = "tinyint";
        public const string BOOLEAN = "boolean";
        public const string DATE = "date";
        public const string DECIMAL = "decimal";
        public const string REAL = "real";
        public const string DOUBLE = "double";
        public const string HyperLogLog = "HyperLogLog";
        public const string QDIGEST = "qdigest";
        public const string P4HyperLogLog = "P4HyperLogLog";
        public const string IntervalDayToSecond = "interval day to second";
        public const string IntervalYearToMonth = "interval year to month";
        public const string TIMESTAMP = "timestamp";
        public const string TimestampWithTimeZone = "timestamp with time zone";
        public const string TIME = "time";
        public const string TimeWithTimeZone = "time with time zone";
        public const string VARBINARY = "varbinary";
        public const string VARCHAR = "varchar";
        public const string CHAR = "char";
        public const string ROW = "row";
        public const string ARRAY = "array";
        public const string MAP = "map";
        public const string JSON = "json";
        public const string IPADDRESS = "ipaddress";
        public const string UUID = "uuid";
        public const string GEOMETRY = "Geometry";
        public const string SphericalGeography = "SphericalGeography";
        public const string BingTile = "BingTile";

        private ClientStandardTypes()
        {
        }
    }
}
