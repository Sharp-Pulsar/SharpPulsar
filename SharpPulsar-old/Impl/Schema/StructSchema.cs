using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Interface.Schema;
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
	using Schema = org.apache.avro.Schema;
	using Parser = org.apache.avro.Schema.Parser;
	using ReflectData = org.apache.avro.reflect.ReflectData;
	using Hex = org.apache.commons.codec.binary.Hex;
	using SerializationException = org.apache.commons.

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

		//protected internal static readonly Logger LOG = LoggerFactory.getLogger(typeof(StructSchema));

		protected internal readonly Schema schema;
		protected internal readonly SchemaInfo schemaInfo;
		protected internal ISchemaReader<T> reader;
		protected internal SchemaWriter<T> writer;
		protected internal ISchemaInfoProvider schemaInfoProvider;

		private readonly LoadingCache<BytesSchemaVersion, ISchemaReader<T>> readerCache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		private class CacheLoaderAnonymousInnerClass : CacheLoader<BytesSchemaVersion, SchemaReader<T>>
		{
			public ISchemaReader<T> Load(BytesSchemaVersion schemaVersion)
			{
				return outerInstance.loadReader(schemaVersion);
			}
		}

		protected internal StructSchema(SchemaInfo schemaInfo)
		{
			this.schema = ParseAvroSchema(new string(schemaInfo.Schema, UTF_8));
			this.schemaInfo = schemaInfo;
		}

		public virtual Schema AvroSchema
		{
			get
			{
				return schema;
			}
		}

		public sbyte[] Encode(T message)
		{
			return writer.write(message);
		}

		public T Decode(sbyte[] bytes)
		{
			return reader.Read(bytes);
		}

		public T Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			try
			{
				return readerCache.get(BytesSchemaVersion.of(schemaVersion)).read(bytes);
			}
			catch (System.Exception e) when (e is ExecutionException || e is AvroTypeException)
			{
				if (e is AvroTypeException)
				{
					throw new SchemaSerializationException(e);
				}
				LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.TopicName, Hex.encodeHexString(schemaVersion), e);
				throw new System.Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public override T Decode(ByteBuf byteBuf)
		{
			return reader.Read(new ByteBufInputStream(byteBuf));
		}

		public override T Decode(ByteBuf byteBuf, sbyte[] schemaVersion)
		{
			try
			{
				return readerCache.get(BytesSchemaVersion.of(schemaVersion)).read(new ByteBufInputStream(byteBuf));
			}
			catch (ExecutionException e)
			{
				LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.TopicName, Hex.encodeHexString(schemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public SchemaInfo SchemaInfo
		{
			get
			{
				return this.schemaInfo;
			}
		}

		protected internal static Schema CreateAvroSchema(ISchemaDefinition<T> schemaDefinition)
		{
			Type pojo = schemaDefinition.Pojo;

			if (!string.IsNullOrWhiteSpace(schemaDefinition.JsonDef))
			{
				return ParseAvroSchema(schemaDefinition.JsonDef);
			}
			else if (pojo != null)
			{
				ThreadLocal<bool> validateDefaults = null;

				try
				{
					System.Reflection.FieldInfo validateDefaultsField = typeof(Schema).getDeclaredField("VALIDATE_DEFAULTS");
					validateDefaultsField.Accessible = true;
					validateDefaults = (ThreadLocal<bool>) validateDefaultsField.get(null);
				}
				catch (Exception e) when (e is NoSuchFieldException || e is IllegalAccessException)
				{
					throw new Exception("Cannot disable validation of default values", e);
				}
				bool savedValidateDefaults = validateDefaults.get();

				try
				{
					// Disable validation of default values for compatibility
					validateDefaults.set(false);
					return schemaDefinition.AlwaysAllowNull ? ReflectData.AllowNull.get().getSchema(pojo) : ReflectData.get().getSchema(pojo);
				}
				finally
				{
					validateDefaults.set(savedValidateDefaults);
				}
			}
			else
			{
				throw new Exception("Schema definition must specify pojo class or schema json definition");
			}
		}

		protected internal static Schema ParseAvroSchema(string schemaJson)
		{
			Schema.Parser parser = new Schema.Parser();
			parser.ValidateDefaults = false;
			return parser.parse(schemaJson);
		}

		protected internal static SchemaInfo ParseSchemaInfo<T>(ISchemaDefinition<T> schemaDefinition, SchemaType schemaType)
		{
			return ISchemaInfo.Builder<T>().schema(CreateAvroSchema(schemaDefinition).ToString().GetBytes(UTF_8)).properties(schemaDefinition.Properties).name("").type(schemaType).build();
		}

		public virtual ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.schemaInfoProvider = value;
			}
		}

		/// <summary>
		/// Load the schema reader for reading messages encoded by the given schema version.
		/// </summary>
		/// <param name="schemaVersion"> the provided schema version </param>
		/// <returns> the schema reader for decoding messages encoded by the provided schema version. </returns>
		protected internal abstract ISchemaReader<T> LoadReader(BytesSchemaVersion schemaVersion);

		/// <summary>
		/// TODO: think about how to make this async
		/// </summary>
		protected internal virtual SchemaInfo GetSchemaInfoByVersion(sbyte[] schemaVersion)
		{
			try
			{
				return schemaInfoProvider.GetSchemaByVersion(schemaVersion).Result;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new SerializationException("Interrupted at fetching schema info for " + SchemaUtils.getStringSchemaVersion(schemaVersion), e);
			}
			catch (ExecutionException e)
			{
				throw new SerializationException("Failed at fetching schema info for " + SchemaUtils.getStringSchemaVersion(schemaVersion), e.InnerException);
			}
		}

		protected internal virtual SchemaWriter<T> Writer
		{
			set
			{
				this.writer = value;
			}
		}

		protected internal virtual ISchemaReader<T> Reader
		{
			set
			{
				this.reader = value;
			}
			get
			{
				return reader;
			}
		}


	}

}