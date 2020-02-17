using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Avro;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities.Encoders;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Builder;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;

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

	/// <summary>
	/// This is a base schema implementation for `Struct` types.
	/// A struct type is used for presenting records (objects) which
	/// have multiple fields.
	/// 
	/// <para>Currently Pulsar supports 3 `Struct` types -
	/// <seealso cref="SchemaType.Avro"/>,
	/// <seealso cref="SchemaType.Json"/>,
	/// and <seealso cref="SchemaType.Protobuf"/>.
	/// </para>
	/// </summary>
	public abstract class StructSchema<T> : AbstractSchema<T>
	{

		protected internal static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(StructSchema<T>));

		protected internal readonly Avro.Schema Schema;
		private readonly SchemaInfo _schemaInfo;
        private ISchemaReader<T> _reader;
        private ISchemaWriter<T> _writer;
        private ISchemaInfoProvider _schemaInfoProvider;
		private readonly ConcurrentDictionary<BytesSchemaVersion, ISchemaReader<T>> _readerCache = new ConcurrentDictionary<BytesSchemaVersion, ISchemaReader<T>>();

        protected StructSchema(SchemaInfo schemaInfo)
		{
			this.Schema = ParseAvroSchema(new string(Encoding.UTF8.GetString((byte[])(object)schemaInfo.Schema)));
			_schemaInfo = schemaInfo;
		}

		public virtual Avro.Schema AvroSchema => Schema;

        public override sbyte[] Encode(T message)
		{
			return _writer.Write(message);
		}

		public override T Decode(sbyte[] bytes)
		{
			return _reader.Read(bytes);
		}

		public override T Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			try
			{
				return _readerCache[BytesSchemaVersion.Of(schemaVersion)].Read(bytes);
			}
			catch (System.Exception e)
			{
				if (e is AvroTypeException)
				{
					throw new SchemaSerializationException(e);
				}
				Log.LogError("Can't get generic schema for topic {} schema version {}", _schemaInfoProvider.TopicName, Hex.Encode((byte[])(object)schemaVersion), e);
				throw new System.Exception("Can't get generic schema for topic " + _schemaInfoProvider.TopicName);
			}
		}

		public override T Decode(IByteBuffer byteBuf)
        {
            var msg = (sbyte[])(object)byteBuf.GetIoBuffers(byteBuf.ReaderIndex, byteBuf.ReadableBytes).ToArray();
			return _reader.Read(msg);
		}

		public override T Decode(IByteBuffer byteBuf, sbyte[] schemaVersion)
		{
			try
			{
                var msg = (sbyte[])(object)byteBuf.GetIoBuffers(byteBuf.ReaderIndex, byteBuf.ReadableBytes).ToArray();
				return _readerCache[BytesSchemaVersion.Of(schemaVersion)].Read(msg);
            }
			catch (System.Exception e)
			{
				Log.LogError("Can't get generic schema for topic {} schema version {}", _schemaInfoProvider.TopicName, Hex.Encode((byte[])(object)schemaVersion), e);
				throw new System.Exception("Can't get generic schema for topic " + _schemaInfoProvider.TopicName);
			}
		}

		public override ISchemaInfo SchemaInfo => _schemaInfo;

        protected internal static Avro.Schema CreateAvroSchema(ISchemaDefinition<T> schemaDefinition)
		{
			var pojo = schemaDefinition.Pojo;

			if (!string.IsNullOrWhiteSpace(schemaDefinition.JsonDef))
			{
				return ParseAvroSchema(schemaDefinition.JsonDef);
			}
			else if (pojo != null)
			{
                throw new System.Exception("Not yet implemented"); //schemaDefinition.AlwaysAllowNull ? Avro.Reflect.AllowNull.get().getSchema(Pojo) : ReflectData.get().getSchema(Pojo);
			}
			else
			{
				throw new System.Exception("Schema definition must specify pojo class or schema json definition");
			}
		}

		protected internal static Avro.Schema ParseAvroSchema(string schemaJson)
		{
            return Avro.Schema.Parse(schemaJson);
		}

		protected internal static SchemaInfo ParseSchemaInfo(ISchemaDefinition<T> schemaDefinition, SchemaType schemaType)
		{
			return new SchemaInfoBuilder().SetSchema(CreateAvroSchema(schemaDefinition).ToString().GetBytes()).SetProperties(schemaDefinition.Properties).SetName("").SetType(schemaType).Build();
		}

		public override ISchemaInfoProvider SchemaInfoProvider
		{
			set => _schemaInfoProvider = value;
        }

		/// <summary>
		/// Load the schema reader for reading messages encoded by the given schema version.
		/// </summary>
		/// <param name="schemaVersion"> the provided schema version </param>
		/// <returns> the schema reader for decoding messages encoded by the provided schema version. </returns>
		public abstract ISchemaReader<T> LoadReader(BytesSchemaVersion schemaVersion);

		/// <summary>
		/// TODO: think about how to make this async
		/// </summary>
		public virtual SchemaInfo GetSchemaInfoByVersion(sbyte[] schemaVersion)
		{
			try
			{
				return (SchemaInfo)_schemaInfoProvider.GetSchemaByVersion(schemaVersion).Result;
			}
			catch (ThreadInterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new SerializationException("Interrupted at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(schemaVersion), e);
			}
			catch (System.Exception e)
			{
				throw new SerializationException("Failed at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(schemaVersion), e.InnerException);
			}
		}

		public  ISchemaWriter<T> Writer
		{
			set => _writer = value;
        }

		public  ISchemaReader<T> Reader
		{
			set => _reader = value;
            get => _reader;
        }


	}

}