using Newtonsoft.Json;
using SharpPulsar.Presto.Facebook.Type;

namespace SharpPulsar.Presto.Facebook
{
    public class TimeZoneKeyConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TimeZoneKey key = (TimeZoneKey)value;

            writer.WriteRawValue(key.Key.ToString());
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = reader.Value.ToString();

            var value = short.Parse(temp);

            return TimeZoneKey.GetTimeZoneKey(value);
        }
    }
}
