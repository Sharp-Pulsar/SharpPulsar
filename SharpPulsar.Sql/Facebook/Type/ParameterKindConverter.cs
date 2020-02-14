using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class ParameterKindConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return (objectType == typeof(ParameterKind));
        }

        public override bool CanWrite => true;

        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = reader.Value.ToString();

            if (String.IsNullOrEmpty(temp))
            {
                return ParameterKind.Variable;
            }
            else
            {
                var matchingEnums = Enum.GetValues(typeof(ParameterKind)).Cast<ParameterKind>().Where(x => x.GetType().GetMember(x.ToString()).FirstOrDefault().GetCustomAttribute<DescriptionAttribute>().Description == temp);

                if (matchingEnums.Any())
                {
                    return matchingEnums.First();
                }
                else
                {
                    return ParameterKind.Variable;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (ParameterKind)value;

            writer.WriteRawValue(val.GetType().GetCustomAttribute<DescriptionAttribute>().Description);
        }
    }
}
