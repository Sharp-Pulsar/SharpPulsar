using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.ConnectorSession.java
    /// </summary>
    [JsonConverter(typeof(DynamicInterfaceConverter))]
    public interface IConnectorSession
    {
        string GetQueryId();

        string GetSource();

        string GetUser();

        Identity GetIdentity();

        TimeZoneKey GetTimeZoneKey();

        CultureInfo GetLocale();

        long GetStartTime();

        object GetProperty(string name, System.Type type);
    }
}
