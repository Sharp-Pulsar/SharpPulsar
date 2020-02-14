using Newtonsoft.Json;

namespace SharpPulsar.Sql.Facebook
{
    public class DynamicInterfaceConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<dynamic>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
