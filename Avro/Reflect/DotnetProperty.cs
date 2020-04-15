/*  
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Avro.Schemas;

namespace Avro.Reflect
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class DotnetProperty
    {
        private PropertyInfo _property;

        public IAvroFieldConverter Converter { get; set; }

        private bool IsPropertyCompatible(Schema.Type schemaTag)
        {
            Type propType;

            if (this.Converter == null)
            {
                propType = this._property.PropertyType;
            }
            else
            {
                propType = this.Converter.GetAvroType();
            }

            switch (schemaTag)
            {
                case Schema.Type.Null:
                    return (Nullable.GetUnderlyingType(propType) != null) || (!propType.IsValueType);
                case Schema.Type.Boolean:
                    return propType == typeof(bool);
                case Schema.Type.Int:
                    return propType == typeof(int);
                case Schema.Type.Long:
                    return propType == typeof(long);
                case Schema.Type.Float:
                    return propType == typeof(float);
                case Schema.Type.Double:
                    return propType == typeof(double);
                case Schema.Type.Bytes:
                    return propType == typeof(byte[]);
                case Schema.Type.String:
                    return typeof(string).IsAssignableFrom(propType);
                case Schema.Type.Record:
                    //TODO: this probably should work for struct too
                    return propType.IsClass;
                case Schema.Type.Enumeration:
                    return propType.IsEnum;
                case Schema.Type.Array:
                    return typeof(IEnumerable).IsAssignableFrom(propType);
                case Schema.Type.Map:
                    return typeof(IDictionary).IsAssignableFrom(propType);
                case Schema.Type.Union:
                    return true;
                case Schema.Type.Fixed:
                    return propType == typeof(byte[]);
                case Schema.Type.Error:
                    return propType.IsClass;
            }

            return false;
        }

        public DotnetProperty(PropertyInfo property, Schema.Type schemaTag,  IAvroFieldConverter converter, ClassCache cache)
        {
            this._property = property;
            this.Converter = converter;

            if (!this.IsPropertyCompatible(schemaTag))
            {
                if (this.Converter == null)
                {
                    var c = cache.GetDefaultConverter(schemaTag, this._property.PropertyType);
                    if (c != null)
                    {
                        this.Converter = c;
                        return;
                    }
                }

                throw new AvroException($"Property {property.Name} in object {property.DeclaringType} isn't compatible with Avro schema type {schemaTag}");
            }
        }

        public DotnetProperty(PropertyInfo property, Schema.Type schemaTag, ClassCache cache)
            : this(property, schemaTag, null, cache)
        {
        }

        public virtual Type GetPropertyType()
        {
            if (this.Converter != null)
            {
                return this.Converter.GetAvroType();
            }

            return this._property.PropertyType;
        }

        public virtual object GetValue(object o, Schema s)
        {
            if (this.Converter != null)
            {
                return this.Converter.ToAvroType(this._property.GetValue(o), s);
            }

            return this._property.GetValue(o);
        }

        public virtual void SetValue(object o, object v, Schema s)
        {
            if (this.Converter != null)
            {
                this._property.SetValue(o, this.Converter.FromAvroType(v, s));
            }
            else
            {
                this._property.SetValue(o, v);
            }
        }
    }
}
