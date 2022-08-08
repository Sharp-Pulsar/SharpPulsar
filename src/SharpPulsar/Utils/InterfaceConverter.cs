using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpPulsar.Utils
{
    public class InterfaceConverter<TM, TI> : JsonConverter<TI> where TM : class, TI
    {
        public override TI Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TM>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TI value, JsonSerializerOptions options) { }
    }
}
