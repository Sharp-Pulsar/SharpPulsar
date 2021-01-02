using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Pulsar.Api.Schema;
using SharpPulsar.Pulsar.Schema;
using SharpPulsar.Shared;
using System;
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
namespace SharpPulsar.Pulsar.Schema
{
	/// <summary>
	/// A schema for `Boolean`.
	/// </summary>
	public class BooleanSchema : AbstractSchema
	{

		private static readonly BooleanSchema _instance;
        private static readonly ISchemaInfo _schemaInfo;

		static BooleanSchema()
		{
            var info = new SchemaInfo
            {
                Name = "Boolean",
                Type = SchemaType.Boolean,
                Schema = new sbyte[0]
            };
            _schemaInfo = info;
			_instance = new BooleanSchema();
		}

		public static BooleanSchema Of()
		{
			return _instance;
		}

		private void Validate(sbyte[] message)
		{
			if (message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by BooleanSchema is not 1");
			}
		}

		public override sbyte[] Encode(object message)
		{
			if (null == message || !(message is bool b))
			{
				return null;
			}
			else
			{
				return new sbyte[]{(sbyte)(b ? 1 : 0)};
			}
		}

		public override T Decode<T>(sbyte[] bytes, T returnType)
        {
			if (null == bytes)
			{
				return default;
			}
			Validate(bytes);
			return (T)(object)(bytes[0] != 0);
		}

		public override T Decode<T>(byte[] byteBuf)
		{
			if (null == byteBuf)
			{
				return default;
			}
			return (T)byteBuf.GetValue(0);
		}

		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}

}