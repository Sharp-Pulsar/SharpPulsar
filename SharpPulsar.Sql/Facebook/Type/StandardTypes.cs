using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class StandardTypes
    {
        #region Public Properties
        public static readonly string Bigint = "bigint";
        public static readonly string Integer = "integer";
        public static readonly string Smallint = "smallint";
        public static readonly string Tinyint = "tinyint";
        public static readonly string Boolean = "boolean";
        public static readonly string Date = "date";
        public static readonly string Decimal = "decimal";
        public static readonly string Real = "real";
        public static readonly string Double = "double";
        public static readonly string HyperLogLog = "HyperLogLog";
        public static readonly string P4HyperLogLog = "P4HyperLogLog";
        public static readonly string IntervalDayToSecond = "interval day to second";
        public static readonly string IntervalYearToMonth = "interval year to month";
        public static readonly string Timestamp = "timestamp";
        public static readonly string TimestampWithTimeZone = "timestamp with time zone";
        public static readonly string Time = "time";
        public static readonly string TimeWithTimeZone = "time with time zone";
        public static readonly string Varbinary = "varbinary";
        public static readonly string Varchar = "varchar";
        public static readonly string Char = "char";
        public static readonly string Row = "row";
        public static readonly string Array = "array";
        public static readonly string Map = "map";
        public static readonly string Json = "json";
        public static readonly string Ipaddress = "ipaddress";
        public static readonly string Geometry = "Geometry";
        public static readonly string BingTile = "BingTile";

        #endregion

        #region Constructors

        private StandardTypes()
        { }
        #endregion
    }
}
