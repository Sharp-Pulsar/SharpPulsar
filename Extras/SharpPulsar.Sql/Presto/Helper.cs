using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpPulsar.Sql.Presto
{
    internal class Helper
    {
        internal static T HttpRequest<T>(string Method, string Url, string Request = "", WebHeaderCollection Headers = null)
        {
            var http = WebRequest.Create(new Uri(Url));
            http.Method = Method;

            if (Headers != null)
                http.Headers.Add(Headers);
            http.Credentials = CredentialCache.DefaultCredentials;

            if (!string.IsNullOrEmpty(Request))
            {
                var encoding = new ASCIIEncoding();
                var bytes = encoding.GetBytes(Request);

                using var newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();
            }
            try
            {
                using var response = http.GetResponse();
                using var stream = response.GetResponseStream();
                if (stream != null)
                {
                    using var sr = new StreamReader(stream);
                    var str = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(str);
                }
                throw new NullReferenceException();
            }
            catch (WebException ex)
            {
                if (ex.Response.ContentLength > 0)
                {
                    using var stream = ex.Response.GetResponseStream();
                    var sr = new StreamReader(stream);
                    throw new Exception("ERROR: " + sr.ReadToEnd());
                }

                throw new Exception("ERROR: " + ex.Message);

            }
        }

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
