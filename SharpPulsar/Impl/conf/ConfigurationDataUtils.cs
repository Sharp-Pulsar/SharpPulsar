using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
	/// <summary>
	/// Utils for loading configuration data.
	/// </summary>
	public sealed class ConfigurationDataUtils
	{

		public static ObjectMapper Create()
		{
			return new ObjectMapper();
		}

		private static readonly FastThreadLocal<ObjectMapper> Mapper = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
			public ObjectMapper InitialValue()
			{
				return Create();
			}
		}

		public static ObjectMapper ThreadLocal
		{
			get
			{
				return Mapper.Value;
			}
		}

		private ConfigurationDataUtils()
		{
		}

		public static T LoadData<T>(IDictionary<string, object> config, T existingData, Type dataCls)
		{
			var mapper = ThreadLocal;
			try
			{
				var existingConfigJson = mapper.WriteValueAsString(existingData);
				var existingConfig = (IDictionary<string, object>)mapper.ReadValue(existingConfigJson, typeof(IDictionary<string, object>));
				IDictionary<string, object> newConfig = new Dictionary<string, object>();
				existingConfig.ToList().ForEach(x=> newConfig.Add(x.Key, x.Value));
				config.ToList().ForEach(x => newConfig.Add(x.Key, x.Value));
				var configJson = mapper.WriteValueAsString(newConfig);
				return (T)mapper.ReadValue(configJson, dataCls);
			}
			catch (IOException e)
			{
				throw new System.Exception("Failed to load config into existing configuration data", e);
			}

		}

	}
	
}