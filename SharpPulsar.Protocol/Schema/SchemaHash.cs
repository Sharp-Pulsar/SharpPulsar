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
	using HashCode = com.google.common.hash.HashCode;
	using HashFunction = com.google.common.hash.HashFunction;
	using Hashing = com.google.common.hash.Hashing;
	using EqualsAndHashCode = lombok.EqualsAndHashCode;
	using Org.Apache.Pulsar.Client.Api;
	using SchemaInfo = SharpPulsar.Schema.SchemaInfo;

	/// <summary>
	/// Schema hash wrapper with a HashCode inner type.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @EqualsAndHashCode public class SchemaHash
	public class SchemaHash
	{

		private static HashFunction hashFunction = Hashing.sha256();

		private readonly HashCode hash;

		private SchemaHash(HashCode Hash)
		{
			this.hash = Hash;
		}

		public static SchemaHash Of(Schema Schema)
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return Of(Optional.ofNullable(Schema).map(Schema::getSchemaInfo).map(SchemaInfo.getSchema).orElse(new sbyte[0]));
		}

		public static SchemaHash Of(SchemaData SchemaData)
		{
			return Of(SchemaData.Data);
		}

		private static SchemaHash Of(sbyte[] SchemaBytes)
		{
			return new SchemaHash(hashFunction.hashBytes(SchemaBytes));
		}

		public virtual sbyte[] AsBytes()
		{
			return hash.asBytes();
		}
	}

}