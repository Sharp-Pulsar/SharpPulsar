using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Schema;
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
namespace SharpPulsar.Schemas
{
    /// <summary>
    /// A schema for `Boolean`.
    /// </summary>
    public class BooleanSchema : AbstractSchema<bool>
	{

		private static readonly BooleanSchema _instance;
        private static readonly ISchemaInfo _schemaInfo;

		static BooleanSchema()
		{
            var info = new SchemaInfo
            {
                Name = "Boolean",
                Type = SchemaType.BOOLEAN,
                Schema = new byte[0]
            };
            _schemaInfo = info;
			_instance = new BooleanSchema();
		}

		public static BooleanSchema Of()
		{
			return _instance;
		}

		public override void Validate(byte[] message)
		{
			if (message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by BooleanSchema is not 1");
			}
		}

		public override byte[] Encode(bool message)
		{
			return new byte[] { (byte)(message ? 1 : 0) };
		}

		public override bool Decode(byte[] bytes)
        {
			Validate(bytes);
			return bytes[0] != 0;
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