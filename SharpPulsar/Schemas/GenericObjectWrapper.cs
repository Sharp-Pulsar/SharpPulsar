using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Shared;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Schemas
{
    /// <summary>
    /// Implementation of GenericRecord that wraps objects of non Struct types.
    /// </summary>
    internal class GenericObjectWrapper : IGenericRecord
    {

        private readonly object _nativeObject;
        private readonly SchemaType _schemaType;
        private readonly byte[] _schemaVersion;

        internal static GenericObjectWrapper Of(object nativeObject, SchemaType schemaType, byte[] schemaVersion)
        {
            return new GenericObjectWrapper(nativeObject, schemaType, schemaVersion);
        }

        private GenericObjectWrapper(object nativeObject, SchemaType schemaType, byte[] schemaVersion)
        {
            _nativeObject = nativeObject;
            _schemaType = Precondition.Condition.RequireNonNull(schemaType, "SchemaType is required");
            _schemaVersion = schemaVersion;
        }

        public byte[] SchemaVersion
        {
            get
            {
                return _schemaVersion;
            }
        }

        public IList<Field> Fields
        {
            get
            {
                return new List<Field>();
            }
        }

        public object GetField(string fieldName)
        {
            return null;
        }
        public object GetField(int pos)
        {
            return null;
        }

        public SchemaType SchemaType
        {
            get
            {
                return _schemaType;
            }
        }

        public object NativeObject
        {
            get
            {
                return _nativeObject;
            }
        }

        public override string ToString()
        {
            return _nativeObject.ToString();
        }

        public override int GetHashCode()
        {
            return _nativeObject.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (!(other is GenericObjectWrapper))
            {
                return false;
            }
            GenericObjectWrapper gw = (GenericObjectWrapper)other;
            return _schemaType == gw._schemaType && Equals(_nativeObject, gw._nativeObject) && _schemaVersion.SequenceEqual(gw._schemaVersion);
        }
    }
}
