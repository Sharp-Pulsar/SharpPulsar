using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using PrestoSharp;

namespace SharpPulsar.Presto
{
    internal class StandardTypes
    {
        static Dictionary<string, Type> _mTypeMapping = new Dictionary<string, Type>();
        static StandardTypes()
        {
            _mTypeMapping.Add(Bigint, typeof(long));
            _mTypeMapping.Add(Integer, typeof(int));
            _mTypeMapping.Add(Smallint, typeof(short));
            _mTypeMapping.Add(Tinyint, typeof(short));
            _mTypeMapping.Add(Boolean, typeof(bool));
            _mTypeMapping.Add(Date, typeof(DateTime));
            _mTypeMapping.Add(Decimal, typeof(decimal));
            _mTypeMapping.Add(Real, typeof(float));
            _mTypeMapping.Add(Double, typeof(double));
            // TODO: Map HYPER_LOG_LOG
            // TODO: Map QDIGEST
            // TODO: Map P4_HYPER_LOG_LOG
            _mTypeMapping.Add(IntervalDayToSecond, typeof(TimeSpan));
            _mTypeMapping.Add(IntervalYearToMonth, typeof(TimeSpan));
            _mTypeMapping.Add(Timestamp, typeof(DateTime));
            _mTypeMapping.Add(TimestampWithTimeZone, typeof(DateTime));
            _mTypeMapping.Add(Time, typeof(DateTime));
            _mTypeMapping.Add(TimeWithTimeZone, typeof(DateTime));
            _mTypeMapping.Add(Varbinary, typeof(byte[]));
            _mTypeMapping.Add(Varchar, typeof(string));
            _mTypeMapping.Add(Char, typeof(string));
            // TODO: Map ROW
            // TODO: Map ARRAY
            // TODO: Map MAP
            // TODO: Map JSON
            _mTypeMapping.Add(Ipaddress, typeof(IPAddress));
            // TODO: Map GEOMETRY
            // TODO: Map BING_TILE

        }

        public static string Bigint = "bigint";
        public static string Integer = "integer";
        public static string Smallint = "smallint";
        public static string Tinyint = "tinyint";
        public static string Boolean = "boolean";
        public static string Date = "date";
        public static string Decimal = "decimal";
        public static string Real = "real";
        public static string Double = "double";
        public static string HyperLogLog = "HyperLogLog";
        public static string Qdigest = "qdigest";
        public static string P4HyperLogLog = "P4HyperLogLog";
        public static string IntervalDayToSecond = "interval day to second";
        public static string IntervalYearToMonth = "interval year to month";
        public static string Timestamp = "timestamp";
        public static string TimestampWithTimeZone = "timestamp with time zone";
        public static string Time = "time";
        public static string TimeWithTimeZone = "time with time zone";
        public static string Varbinary = "varbinary";
        public static string Varchar = "varchar";
        public static string Char = "char";
        public static string Row = "row";
        public static string Array = "array";
        public static string Map = "map";
        public static string Json = "json";
        public static string Ipaddress = "ipaddress";
        public static string Geometry = "Geometry";
        public static string BingTile = "BingTile";

        internal static Type MapType(string typeName)
        {
            if (_mTypeMapping.ContainsKey(typeName))
                return _mTypeMapping[typeName];
            return Type.Missing.GetType();
        }

        internal static object Convert(string typeName, object obj)
        {
            if (typeName == Varbinary)
                return System.Convert.FromBase64String((string)obj);
            if (typeName == TimeWithTimeZone)
                return Helper.ParseTimeWithTimeZone((string)obj);
            if (typeName == TimestampWithTimeZone)
                return Helper.ParseTimeWithTimeZone((string)obj);
            if (typeName == IntervalYearToMonth)
                return Helper.ParseIntervalYearToMonth((string)obj);
            if (typeName == IntervalDayToSecond)
                return Helper.ParseIntervalDayToSecond((string)obj);
            if (typeName == Array)
                return ((JArray)obj).ToObject<object[]>();
            if (typeName == Map)
                return Helper.ParseDictionary((JObject)obj);
            if (typeName == Row)
                return obj; // TODO: Parse Row
            if (typeName == Json)
                return ((JToken)obj);
            if (typeName == Ipaddress)
                return Helper.ParseIpAddress((string)obj);
            return System.Convert.ChangeType(obj, MapType(typeName));
        }

    }
}
