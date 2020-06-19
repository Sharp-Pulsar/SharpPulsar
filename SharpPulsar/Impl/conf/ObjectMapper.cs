using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Newtonsoft.Json.Linq;

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
namespace SharpPulsar.Impl.Conf
{
    public class ObjectMapper
	{
		public  JsonSerializerOptions Options()
		{
			var jsonObject = new JsonSerializerOptions
			{
				IgnoreNullValues = true,
				WriteIndented = true,
                MaxDepth = 256
			};
			return jsonObject;
		}

        public byte[] WriteValueAsBytes(object @object)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@object, Options());
        }
		public ObjectMapper WithOutAttribute(object @object)
		{
			return this;
		}
		public object ReadValue(string existingConfigJson, Type t)
		{
			return JsonSerializer.Deserialize(existingConfigJson, t);
		}
        public object ReadValue(string existingConfigJson, Type t, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(existingConfigJson, t, options);
        }
		public JToken ReadValue(string existingConfigJson)
        {
            return JToken.Parse(existingConfigJson);
        }
		public object ReadValue(byte[] param, int position, long length)
        {
			using var ms = new MemoryStream(param)
			{
				Position = position
			};
			ms.SetLength(length);
			IFormatter br = new BinaryFormatter();
			return br.Deserialize(ms);
		}
        public JToken ReadValue(Stream stream)
        {
            using (stream)
            {
                IFormatter br = new BinaryFormatter();
                var st = br.Deserialize(stream).ToString();
				return JToken.Parse(st);
            }
        }
	}
	
}