
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using SharpPulsar.Common;

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
namespace SharpPulsar.Protocol.Schema
{

    /// <summary>
    /// Stored schema with version.
    /// </summary>
    public class StoredSchema:  IEquatable<StoredSchema>
    {
        public readonly sbyte[] Data;
        public readonly ISchemaVersion Version;

        public StoredSchema(sbyte[] data, ISchemaVersion version)
        {
            this.Data = data;
            this.Version = version;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }
            StoredSchema That = (StoredSchema)o;
            return Data.SequenceEqual(That.Data) && object.Equals(Version, That.Version);
        }

        public bool Equals(StoredSchema other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {

            int Result = Version.GetHashCode();
            Result = 31 * Result + Data.GetHashCode();
            return Result;
        }

        public override string ToString()
        {
            return StringHelper
                .Build(this)
            .Add("data", Data)
            .Add("version", Version)
            .ToString();
        }
    }
}
