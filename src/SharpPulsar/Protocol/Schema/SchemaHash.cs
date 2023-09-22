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
    
    using SharpPulsar.Interfaces;
    using System;
    using System.Security.Cryptography;

    //using SchemaInfo = SharpPulsar.Schemas.SchemaInfo;

    /// <summary>
    /// Schema hash wrapper with a HashCode inner type.
    /// </summary>
    public class SchemaHash
	{

		private readonly byte[] _hash;

		private SchemaHash(byte[] hash)
		{
			this._hash = hash;
		}

		public static SchemaHash Of<T>(ISchema<T> schema)
		{
			var schem = schema != null? schema.SchemaInfo.Schema: Array.Empty<byte>();
			return Of(schem);
		}

		public static SchemaHash Of(SchemaData schemaData)
		{
			return Of(schemaData.Data);
		}

		private static SchemaHash Of(byte[] schemaBytes)
		{
			var scmBy = (byte[])(Array)schemaBytes;
			return new SchemaHash(SHA256.Create().ComputeHash(scmBy));
		}

		public virtual byte[] AsBytes()
		{
			var scmBy = (byte[])(Array)_hash;
			return scmBy;
		}
	}

}