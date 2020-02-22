using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SharpPulsar.Impl.Schema
{//https://docs.oracle.com/database/nosql-12.1.3.0/GettingStartedGuide/avroschemas.html
    public static class GenerateSchema
    {
        public static string GetSchema(this Type type)
        {
            var schema = new Dictionary<string, object>
            {
                {"type", "record"}, {"namespace", type.Namespace}, {"name", type.Namespace + "." + type.Name}
            };
            var propertiesCollections = new List<Dictionary<string,object>>(); 
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType.Namespace != null && (p.PropertyType.IsClass && !p.PropertyType.Namespace.StartsWith("System")))
                {
                    var prop = GetClassProperties(p);
                    var d = new Dictionary<string, object> { { "name", p.Name }, { "type", prop } };
                    propertiesCollections.Add(d);
                }
                else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                {
                    var cp = new Dictionary<string, object> { { "type", "array" }, { "items", ToAvroDataType(p.PropertyType.GetGenericArguments()[0].Name) } };
                    propertiesCollections.Add(cp);
                }
                else if(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                {
                    var dp = new Dictionary<string, object> { { "type", "map" }, { "values", ToAvroDataType(p.PropertyType.GetGenericArguments()[1].Name )} };
                    propertiesCollections.Add(dp);
                }
                else if (p.PropertyType.IsEnum)
                {
                    var dp = new Dictionary<string, object> { { "type", "enum" }, { "name", p.PropertyType.Namespace + "." + p.PropertyType.Name }, { "namespace", p.PropertyType.Namespace }, { "symbols", GetEnumValues(p.PropertyType) } };
                    var e = new Dictionary<string, object> { { "name", p.PropertyType.Name }, { "type", dp } };
                    propertiesCollections.Add(e);
                }
                else
                {
                    var dT = ToAvroDataType(p.PropertyType.Name);
                    var d = new Dictionary<string, object> { { "name", p.Name }, { "type", new List<string>{dT, "null"} } };
                    propertiesCollections.Add(d);
                }
            }
            schema.Add("fields", propertiesCollections);
            return JsonSerializer.Serialize(schema);
        }
        private static Dictionary<string, object> GetClassProperties(PropertyInfo property)
        {
            var schema = new Dictionary<string, object>
            {
                {"type", "record"}, {"namespace", property.PropertyType.Namespace}, {"name", property.PropertyType.Namespace + "." + property.PropertyType.Name}
            };
            var t = property.PropertyType.GetProperties();
            var propertiesCollections = new List<Dictionary<string, object>>();
            var properties = property.PropertyType.GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType.Namespace != null && (p.PropertyType.IsClass && !p.PropertyType.Namespace.StartsWith("System")))
                {
                    var prop = GetClassProperties(p);
                    var d = new Dictionary<string, object> { { "name", p.Name }, { "type", prop } };
                    propertiesCollections.Add(d);
                }
                else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>)))
                {
                    var cp = new Dictionary<string, object> { { "type", "array" }, { "items", ToAvroDataType(p.PropertyType.GetGenericArguments()[0].Name) }};
                    propertiesCollections.Add(cp);
                }
                else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IDictionary<,>)))
                {
                    var dp = new Dictionary<string, object> { { "type", "map" }, { "values", ToAvroDataType(p.PropertyType.GetGenericArguments()[1].Name) } };
                    propertiesCollections.Add(dp);
                }
                else if (p.PropertyType.IsEnum)
                {
                    var dp = new Dictionary<string, object> { { "type", "enum" }, { "name", p.PropertyType.Namespace+"."+p.PropertyType.Name }, { "namespace", p.PropertyType.Namespace }, { "symbols", GetEnumValues(p.PropertyType) } };
                    var e = new Dictionary<string, object> { { "name", p.PropertyType.Name }, {"type", dp} };
                    propertiesCollections.Add(e);
                }
                else
                {
                    var dT = ToAvroDataType(p.PropertyType.Name);
                    var d = new Dictionary<string, object> { { "name", p.Name }, { "type", new List<string> { dT, "null" } } };
                    propertiesCollections.Add(d);
                }
            }
            schema.Add("fields", propertiesCollections);
            return schema;
        }
        private static List<string> GetEnumValues(Type type)
        {
            var list = new List<string>();
            var values = Enum.GetValues(type);
            foreach (var v in values)
            {
                list.Add(v.ToString());
            }
            return list;
        }
        private static string ToAvroDataType(string type)
        {
            switch (type)
            {
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "String":
                    return "string";
                case "Double":
                    return "double";
                case "Single":
                    return "float";
                case "Boolean":
                    return "boolean";
                case "Byte[]":
                case "SByte[]":
                    return "bytes";
                default:
                    throw new ArgumentException($"{type} not supported");
            }
        }
    }
}
