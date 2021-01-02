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

using System;
using SharpPulsar.Api;
using SharpPulsar.Pulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Pulsar.Schema
{
    /// <summary>
	/// A schema for bytes array.
	/// </summary>
	public class BytesSchema : AbstractSchema
	{

		private static readonly BytesSchema Instance = new BytesSchema();
        
        public override ISchema Json(ISchemaDefinition schemaDefinition)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema Json(object pojo)
        {
            throw new System.NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            
        }

        public override bool RequireFetchingSchemaInfo()
        {
            return false;
        }
        
        public override ISchemaInfo SchemaInfo {get;}
        public override bool SupportSchemaVersioning()
        {
            return false;
        }

        public override ISchemaInfoProvider SchemaInfoProvider
        {
            set => throw new System.NotImplementedException();
        }

        public BytesSchema()
		{
            var s = new SchemaInfo {Name = "Bytes", Type = SchemaType.Bytes, Schema = new sbyte[0]};
            SchemaInfo = s;
		}

		public static BytesSchema Of()
		{
			return Instance;
		}

		public override sbyte[] Encode(object message)
		{
            if(!(message is sbyte[]))
                throw new ArgumentException($"{message.GetType()} is not sbyte[]");
			return (sbyte[])message;
		}

        public override void Validate(sbyte[] message, Type returnType)
        {
            
        }

        public override object Decode(sbyte[] bytes, Type returnType)
		{
			return bytes;
		}

		public override object Decode(byte[] byteBuf, Type returnType)
		{
			if (byteBuf == null)
			{
				return null;
			}
			return Convert.ChangeType(byteBuf, returnType);
		}

	}

}