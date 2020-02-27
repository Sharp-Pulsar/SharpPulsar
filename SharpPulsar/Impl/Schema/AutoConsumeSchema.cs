using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Schema.Generic;
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
	/// Auto detect schema.
	/// </summary>
	public class AutoConsumeSchema : ISchema
	{

		private ISchema _schema;

		private string _topicName;

		private string _componentName;

		private ISchemaInfoProvider _schemaInfoProvider;

		public virtual ISchema Schema
		{
			set => this._schema = value;
        }

		private void EnsureSchemaInitialized()
		{
			if(null == _schema)
                throw new NullReferenceException("Schema is not initialized before used");
		}

		public void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			_schema.Validate(message);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		public sbyte[] Encode(object message)
		{
			if(!(message is IGenericRecord))
				throw  new ArgumentException($"{message.GetType()} is not IGenericRecord");
			EnsureSchemaInitialized();

			return _schema.Encode(message);
		}

		public IGenericRecord Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (_schema == null)
			{
				SchemaInfo schemaInfo = null;
				try
				{
					schemaInfo = (SchemaInfo)_schemaInfoProvider.LatestSchema.Result;
				}
				catch (System.Exception e) when (e is ThreadInterruptedException)
				{
                    Thread.CurrentThread.Interrupt();
                    Log.LogError("Con't get last schema for topic {} use AutoConsumeSchema", _topicName);
					throw new SchemaSerializationException(e);
				}
				_schema = GenerateSchema(schemaInfo);
				_schema.SchemaInfoProvider = _schemaInfoProvider;
				Log.LogInformation("Configure {} schema for topic {} : {}", _componentName, _topicName, schemaInfo.SchemaDefinition);
			}
			EnsureSchemaInitialized();
			return (IGenericRecord)_schema.Decode(bytes, schemaVersion);
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

		public virtual ISchemaInfo SchemaInfo => _schema?.SchemaInfo;

        public bool RequireFetchingSchemaInfo()
		{
			return true;
		}

		public  void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			this._topicName = topicName;
			this._componentName = componentName;
            if (schemaInfo == null) return;
            var genericSchema = GenerateSchema(schemaInfo);
            Schema = genericSchema;
            Log.LogInformation("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
        }

		private IGenericSchema GenerateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.Avro && schemaInfo.Type != SchemaType.Json)
			{
				throw new System.Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.Of(schemaInfo, false);
		}

		public static ISchema GetSchema(SchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type.Value)
			{ 
				case -1:
					return BytesSchema.Of();
				case 2:
					return GenericSchemaImpl.Of(schemaInfo);
				default:
					throw new ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<AutoConsumeSchema>();
	}

}