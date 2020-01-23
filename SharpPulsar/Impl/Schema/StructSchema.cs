using System;
using System.Threading;

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
namespace SharpPulsar.Impl.Schema
{


	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufInputStream = io.netty.buffer.ByteBufInputStream;
	using AvroTypeException = org.apache.avro.AvroTypeException;
	using Parser = org.apache.avro.Schema.Parser;
	using ReflectData = org.apache.avro.reflect.ReflectData;
	using Hex = org.apache.commons.codec.binary.Hex;
	using SerializationException = org.apache.commons.lang3.SerializationException;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using SharpPulsar.Api.Schema;
	using SchemaInfoProvider = SharpPulsar.Api.Schema.SchemaInfoProvider;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;
	using BytesSchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.BytesSchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// This is a base schema implementation for `Struct` types.
	/// A struct type is used for presenting records (objects) which
	/// have multiple fields.
	/// 
	/// <para>Currently Pulsar supports 3 `Struct` types -
	/// <seealso cref="org.apache.pulsar.common.schema.SchemaType.AVRO"/>,
	/// <seealso cref="org.apache.pulsar.common.schema.SchemaType.JSON"/>,
	/// and <seealso cref="org.apache.pulsar.common.schema.SchemaType.PROTOBUF"/>.
	/// </para>
	/// </summary>
	public abstract class StructSchema<T> : AbstractSchema<T>
	{

		protected internal static readonly Logger LOG = LoggerFactory.getLogger(typeof(StructSchema));

		protected internal readonly org.apache.avro.Schema Schema;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly SchemaInfo SchemaInfoConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal SchemaReader<T> ReaderConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal SchemaWriter<T> WriterConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal SchemaInfoProvider SchemaInfoProviderConflict;

		private readonly LoadingCache<BytesSchemaVersion, SchemaReader<T>> readerCache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, BAMCIS.Util.Concurrent.TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		public class CacheLoaderAnonymousInnerClass : CacheLoader<BytesSchemaVersion, SchemaReader<T>>
		{
			public override SchemaReader<T> load(BytesSchemaVersion SchemaVersion)
			{
				return outerInstance.loadReader(SchemaVersion);
			}
		}

		public StructSchema(SchemaInfo SchemaInfo)
		{
			this.Schema = ParseAvroSchema(new string(SchemaInfo.Schema, UTF_8));
			this.SchemaInfoConflict = SchemaInfo;
		}

		public virtual org.apache.avro.Schema AvroSchema
		{
			get
			{
				return Schema;
			}
		}

		public override sbyte[] Encode(T Message)
		{
			return WriterConflict.write(Message);
		}

		public override T Decode(sbyte[] Bytes)
		{
			return ReaderConflict.read(Bytes);
		}

		public override T Decode(sbyte[] Bytes, sbyte[] SchemaVersion)
		{
			try
			{
				return readerCache.get(BytesSchemaVersion.of(SchemaVersion)).read(Bytes);
			}
			catch (Exception e) when (e is ExecutionException || e is AvroTypeException)
			{
				if (e is AvroTypeException)
				{
					throw new SchemaSerializationException(e);
				}
				LOG.error("Can't get generic schema for topic {} schema version {}", SchemaInfoProviderConflict.TopicName, Hex.encodeHexString(SchemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + SchemaInfoProviderConflict.TopicName);
			}
		}

		public override T Decode(ByteBuf ByteBuf)
		{
			return ReaderConflict.read(new ByteBufInputStream(ByteBuf));
		}

		public override T Decode(ByteBuf ByteBuf, sbyte[] SchemaVersion)
		{
			try
			{
				return readerCache.get(BytesSchemaVersion.of(SchemaVersion)).read(new ByteBufInputStream(ByteBuf));
			}
			catch (ExecutionException E)
			{
				LOG.error("Can't get generic schema for topic {} schema version {}", SchemaInfoProviderConflict.TopicName, Hex.encodeHexString(SchemaVersion), E);
				throw new Exception("Can't get generic schema for topic " + SchemaInfoProviderConflict.TopicName);
			}
		}

		public override SchemaInfo SchemaInfo
		{
			get
			{
				return this.SchemaInfoConflict;
			}
		}

		protected internal static org.apache.avro.Schema CreateAvroSchema(SchemaDefinition SchemaDefinition)
		{
			Type Pojo = SchemaDefinition.Pojo;

			if (StringUtils.isNotBlank(SchemaDefinition.JsonDef))
			{
				return ParseAvroSchema(SchemaDefinition.JsonDef);
			}
			else if (Pojo != null)
			{
				return SchemaDefinition.AlwaysAllowNull ? ReflectData.AllowNull.get().getSchema(Pojo) : ReflectData.get().getSchema(Pojo);
			}
			else
			{
				throw new Exception("Schema definition must specify pojo class or schema json definition");
			}
		}

		protected internal static org.apache.avro.Schema ParseAvroSchema(string SchemaJson)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.avro.Schema.Parser parser = new org.apache.avro.Schema.Parser();
			Parser Parser = new Parser();
			return Parser.parse(SchemaJson);
		}

		protected internal static SchemaInfo ParseSchemaInfo<T>(SchemaDefinition<T> SchemaDefinition, SchemaType SchemaType)
		{
			return SchemaInfo.builder().schema(CreateAvroSchema(SchemaDefinition).ToString().GetBytes(UTF_8)).properties(SchemaDefinition.Properties).name("").type(SchemaType).build();
		}

		public override SchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.SchemaInfoProviderConflict = value;
			}
		}

		/// <summary>
		/// Load the schema reader for reading messages encoded by the given schema version.
		/// </summary>
		/// <param name="schemaVersion"> the provided schema version </param>
		/// <returns> the schema reader for decoding messages encoded by the provided schema version. </returns>
		public abstract SchemaReader<T> LoadReader(BytesSchemaVersion SchemaVersion);

		/// <summary>
		/// TODO: think about how to make this async
		/// </summary>
		public virtual SchemaInfo GetSchemaInfoByVersion(sbyte[] SchemaVersion)
		{
			try
			{
				return SchemaInfoProviderConflict.getSchemaByVersion(SchemaVersion).get();
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new SerializationException("Interrupted at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(SchemaVersion), E);
			}
			catch (ExecutionException E)
			{
				throw new SerializationException("Failed at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(SchemaVersion), E.InnerException);
			}
		}

		public virtual SchemaWriter<T> Writer
		{
			set
			{
				this.WriterConflict = value;
			}
		}

		public virtual SchemaReader<T> Reader
		{
			set
			{
				this.ReaderConflict = value;
			}
			get
			{
				return ReaderConflict;
			}
		}


	}

}