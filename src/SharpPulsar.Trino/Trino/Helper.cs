using System.Net;
using Newtonsoft.Json.Linq;

namespace SharpPulsar.Trino.Trino
{
    internal class Helper
    {       
        internal static DateTime ParseTimeWithTimeZone(string Data)
        {
            var iSep = Data.IndexOf(' ');

            return DateTime.Parse(Data.Substring(0, iSep));
            // TODO: Agregar TimeZone
        }

        internal static TimeSpan ParseIntervalYearToMonth(string Data)
        {
            var iSep = Data.IndexOf('-');

            return new TimeSpan(Convert.ToInt32(Data.Substring(0, iSep)) * 365 + Convert.ToInt32(Data.Substring(iSep + 1)) * 30, 0, 0, 0);
        }

        internal static TimeSpan ParseIntervalDayToSecond(string Data)
        {
            var iSep = Data.IndexOf(' ');

            return TimeSpan.Parse(Data.Substring(iSep + 1)).Add(new TimeSpan(Convert.ToInt32(Data.Substring(0, iSep)), 0, 0, 0));
        }

        internal static Dictionary<string, object> ParseDictionary(JObject Obj)
        {
            var list = new Dictionary<string, object>();

            foreach (var item in Obj.Properties())
                list.Add(item.Name, item.Value);

            return list;
        }

        internal static IPAddress ParseIpAddress(string Data)
        {
            IPAddress ip;

            if (IPAddress.TryParse(Data, out ip))
                return ip;
            return null;
        }

    }
}
