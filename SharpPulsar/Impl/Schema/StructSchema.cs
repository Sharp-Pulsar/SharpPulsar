using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities.Encoders;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Protocol.Builder;
using SharpPulsar.Protocol.Schema;
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
	public abstract class StructSchema : AbstractSchema
	{

		protected internal static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(StructSchema));

		protected internal readonly Avro.Schema Schema;
		private readonly SchemaInfo _schemaInfo;
        private GenericAvroReader _reader;
        private GenericAvroWriter _writer;
        private ISchemaInfoProvider _schemaInfoProvider;
        private readonly ConcurrentDictionary<BytesSchemaVersion, GenericAvroReader> _readerCache = new ConcurrentDictionary<BytesSchemaVersion, GenericAvroReader>();

		protected StructSchema(SchemaInfo schemaInfo)
		{
			Schema = ParseAvroSchema(new string(Encoding.UTF8.GetString((byte[])(object)schemaInfo.Schema)));
			_schemaInfo = schemaInfo;
		}

		public virtual Avro.Schema AvroSchema => Schema;

        public override sbyte[] Encode(object message)
		{
			if(_writer == null)
				_writer = new GenericAvroWriter(Schema);
			var w = _writer.Write(message);
            return (sbyte[]) (object) w.ToArray();
        }

		public override object Decode(sbyte[] bytes, Type returnType)
		{
			if(_reader == null)
				_reader = new GenericAvroReader(Schema, new sbyte[]{0});
			return _reader.Read((byte[])(object)bytes, returnType);
		}

		
		public override object Decode(byte[] msg, Type returnType)
        {
			return _reader.Read(msg, returnType);
		}

		public override object Decode(byte[] msg, sbyte[] schemaVersion, Type returnType)
		{
			try
			{
				return _readerCache[BytesSchemaVersion.Of(schemaVersion)].Read(msg, returnType);
            }
			catch (System.Exception e)
			{
				Log.LogError("Can't get generic schema for topic {} schema version {}", _schemaInfoProvider.TopicName, Hex.Encode((byte[])(object)schemaVersion), e);
				throw new System.Exception("Can't get generic schema for topic " + _schemaInfoProvider.TopicName);
			}
		}

		public override ISchemaInfo SchemaInfo => _schemaInfo;

        protected internal static Avro.Schema CreateAvroSchema(ISchemaDefinition schemaDefinition)
		{
			var pojo = schemaDefinition.Pojo;

			if (!string.IsNullOrWhiteSpace(schemaDefinition.JsonDef))
			{
				return ParseAvroSchema(schemaDefinition.JsonDef);
			}

            var schema = pojo.GetSchema();
            return ParseAvroSchema(schema);

        }

		protected internal static Avro.Schema ParseAvroSchema(string schemaJson)
		{
            return Avro.Schema.Parse(schemaJson);
		}

		protected internal static SchemaInfo ParseSchemaInfo(ISchemaDefinition schemaDefinition, SchemaType schemaType)
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
		public abstract GenericAvroReader LoadReader(BytesSchemaVersion schemaVersion);

		/// <summary>
		/// TODO: think about how to make this async
		/// </summary>
		public virtual SchemaInfo GetSchemaInfoByVersion(sbyte[] schemaVersion)
		{
			try
			{
				return (SchemaInfo)_schemaInfoProvider.GetSchemaByVersion(schemaVersion);
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

		public  GenericAvroWriter Writer
		{
			set => _writer = value;
        }

		public  GenericAvroReader Reader
		{
			set => _reader = value;
            get => _reader;
        }


	}

}