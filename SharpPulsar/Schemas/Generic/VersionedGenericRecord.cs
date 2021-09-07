using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces.Schema;
using System.Collections.Generic;
using System.Linq;

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
namespace SharpPulsar.Schemas.Generic
{

    /// <summary>
    /// A generic record carrying schema version.
    /// </summary>
    public abstract class VersionedGenericRecord : IGenericRecord
	{
        protected internal readonly byte[] _schemaVersion;
        protected internal readonly IList<Field> _fields;

        protected internal VersionedGenericRecord(byte[] schemaVersion, IList<Field> fields)
        {
            _schemaVersion = schemaVersion;
            _fields = fields;
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
                return _fields;
            }
        }

        public object GetField(string fieldName)
        {
            return _fields.Where(f => f.Name.Equals(fieldName, System.StringComparison.CurrentCultureIgnoreCase));
        }
    }

}