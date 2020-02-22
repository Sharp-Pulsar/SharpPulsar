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
                var parsed = Parse(p);
                if (!parsed.Any())
                    continue;
                propertiesCollections.Add(parsed);
            }
            schema.Add("fields", propertiesCollections);
            return JsonSerializer.Serialize(schema);
        }
        private static List<Dictionary<string, object>> GetClassProperties(PropertyInfo property)
        {
            var t = property.PropertyType.GetProperties();
            var propertiesCollections = new List<Dictionary<string, object>>();
            var properties = property.PropertyType.GetProperties();
            foreach (var p in properties)
            {
                var parsed = Parse(p);
                if (!parsed.Any())
                    continue;
                propertiesCollections.Add(parsed);
            }
            
            return propertiesCollections;
        }
        private static List<Dictionary<string, object>> GetClassProperties(PropertyInfo[] properties)
        {
            var propertiesCollections = new List<Dictionary<string, object>>();
            foreach (var p in properties)
            {
                var parsed = Parse(p);
                if(!parsed.Any())
                    continue;
                propertiesCollections.Add(parsed);
            }

            return propertiesCollections;
        }

        private static Dictionary<string, object> Parse(PropertyInfo property)
        {
            var p = property;
            if (p.PropertyType.Namespace != null && (p.PropertyType.IsClass && !p.PropertyType.Namespace.StartsWith("System")))
            {
                var schema2 = new Dictionary<string, object>
                    {
                        {"type", "record"}, {"namespace", p.PropertyType.Namespace}, {"name", p.PropertyType.Namespace + "." + p.PropertyType.Name}
                    };
                var prop = GetClassProperties(p);
                schema2.Add("fields", prop);
                return new Dictionary<string, object> { { "name", p.Name }, { "type", schema2 } };
                
            }

            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                var v = p.PropertyType.GetGenericArguments()[0];
                if (v.Namespace != null && (v.IsClass && !v.Namespace.StartsWith("System")))
                {
                    var schema2 = new Dictionary<string, object>
                    {
                        {"type", "record"}, {"namespace", v.Namespace}, {"name", v.Namespace + "." + v.Name}
                    };
                    var prop = GetClassProperties(v.GetProperties());
                    schema2.Add("fields", prop);
                    return new Dictionary<string, object> { { "type", "array" },  { "items", schema2 } };
                }
                return new Dictionary<string, object> { { "type", "array" }, { "items", ToAvroDataType(p.PropertyType.GetGenericArguments()[0].Name) } };
            }

            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
            {
                var v = p.PropertyType.GetGenericArguments()[1];
                if (v.Namespace != null && (v.IsClass && !v.Namespace.StartsWith("System")))
                {
                    var schema2 = new Dictionary<string, object>
                    {
                        {"type", "record"}, {"namespace", v.Namespace}, {"name", v.Namespace + "." + v.Name}
                    };
                    var prop = GetClassProperties(v.GetProperties());
                    schema2.Add("fields", prop);
                    return new Dictionary<string, object> { { "type", "map" }, { "values", schema2 } };
                    
                }
                return new Dictionary<string, object> { { "type", "map" }, { "values", ToAvroDataType(p.PropertyType.GetGenericArguments()[1].Name) } };
                
            }

            if (p.PropertyType.IsEnum)
            {
                var dp = new Dictionary<string, object> { { "type", "enum" }, { "name", p.PropertyType.Namespace + "." + p.PropertyType.Name }, { "namespace", p.PropertyType.Namespace }, { "symbols", GetEnumValues(p.PropertyType) } };
                return new Dictionary<string, object> { { "name", p.PropertyType.Name }, { "type", dp } };
                
            }

            var dT = ToAvroDataType(p.PropertyType.Name);
            return new Dictionary<string, object> { { "name", p.Name }, { "type", new List<string> { dT, "null" } } };
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
