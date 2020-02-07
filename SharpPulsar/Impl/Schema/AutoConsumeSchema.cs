using System;
using System.Threading;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Schema.Generic;

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
	/// Auto detect schema.
	/// </summary>
	public class AutoConsumeSchema : ISchema<IGenericRecord>
	{

		private ISchema<IGenericRecord> _schema;

		private string _topicName;

		private string _componentName;

		private ISchemaInfoProvider _schemaInfoProvider;

		public virtual ISchema<IGenericRecord> Schema
		{
			set
			{
				this._schema = value;
			}
		}

		private void EnsureSchemaInitialized()
		{
			checkState(null != _schema, "Schema is not initialized before used");
		}

		public override void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			_schema.Validate(message);
		}

		public override bool SupportSchemaVersioning()
		{
			return true;
		}

		public override sbyte[] Encode(IGenericRecord message)
		{
			EnsureSchemaInitialized();

			return _schema.Encode(message);
		}

		public override IGenericRecord Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (_schema == null)
			{
				SchemaInfo schemaInfo = null;
				try
				{
					schemaInfo = _schemaInfoProvider.LatestSchema.get();
				}
				catch (Exception e) when (e is InterruptedException || e is ExecutionException)
				{
					if (e is InterruptedException)
					{
						Thread.CurrentThread.Interrupt();
					}
					log.error("Con't get last schema for topic {} use AutoConsumeSchema", _topicName);
					throw new SchemaSerializationException(e.Cause);
				}
				_schema = GenerateSchema(schemaInfo);
				_schema.SchemaInfoProvider = _schemaInfoProvider;
				log.info("Configure {} schema for topic {} : {}", _componentName, _topicName, schemaInfo.SchemaDefinition);
			}
			EnsureSchemaInitialized();
			return _schema.Decode(bytes, schemaVersion);
		}

		public virtual ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				if (_schema == null)
				{
					this._schemaInfoProvider = value;
				}
				else
				{
					_schema.SchemaInfoProvider = value;
				}
			}
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				if (_schema == null)
				{
					return null;
				}
				return _schema.SchemaInfo;
			}
		}

		public override bool RequireFetchingSchemaInfo()
		{
			return true;
		}

		public override void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			this._topicName = topicName;
			this._componentName = componentName;
			if (schemaInfo != null)
			{
				var genericSchema = GenerateSchema(schemaInfo);
				Schema = genericSchema;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
			}
		}

		private IGenericSchema GenerateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
			{
				throw new Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.of(schemaInfo, false);
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static SharpPulsar.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
		public static ISchema<object> GetSchema(SchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type)
			{
				case SchemaFields.String:
					return StringSchema.Utf8();
				case SchemaFields.Bytes:
					return BytesSchema.Of();
				case JSON:
					return GenericSchemaImpl.Of(schemaInfo);
				default:
					throw new ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}
	}

}