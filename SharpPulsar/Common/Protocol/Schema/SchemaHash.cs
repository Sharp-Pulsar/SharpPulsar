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
namespace org.apache.pulsar.common.protocol.schema
{
	using HashCode = com.google.common.hash.HashCode;
	using HashFunction = com.google.common.hash.HashFunction;
	using Hashing = com.google.common.hash.Hashing;
	using EqualsAndHashCode = lombok.EqualsAndHashCode;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;

	/// <summary>
	/// Schema hash wrapper with a HashCode inner type.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @EqualsAndHashCode public class SchemaHash
	public class SchemaHash
	{

		private static HashFunction hashFunction = Hashing.sha256();

		private readonly HashCode hash;

		private SchemaHash(HashCode hash)
		{
			this.hash = hash;
		}

		public static SchemaHash of(Schema schema)
		{
			return of(Optional.ofNullable(schema).map(Schema.getSchemaInfo).map(SchemaInfo.getSchema).orElse(new sbyte[0]));
		}

		public static SchemaHash of(SchemaData schemaData)
		{
			return of(schemaData.Data);
		}

		private static SchemaHash of(sbyte[] schemaBytes)
		{
			return new SchemaHash(hashFunction.hashBytes(schemaBytes));
		}

		public virtual sbyte[] asBytes()
		{
			return hash.asBytes();
		}
	}

}