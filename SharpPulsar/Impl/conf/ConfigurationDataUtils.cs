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

		private static readonly FastThreadLocal<ObjectMapper> mapper = new FastThreadLocalAnonymousInnerClass();

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
				return mapper.Value;
			}
		}

		private ConfigurationDataUtils()
		{
		}

		public static T LoadData<T>(IDictionary<string, object> Config, T ExistingData, Type DataCls)
		{
			var Mapper = ThreadLocal;
			try
			{
				string existingConfigJson = Mapper.WriteValueAsString(ExistingData);
				IDictionary<string, object> existingConfig = (IDictionary<string, object>)Mapper.ReadValue(existingConfigJson, typeof(IDictionary<string, object>));
				IDictionary<string, object> newConfig = new Dictionary<string, object>();
				existingConfig.ToList().ForEach(x=> newConfig.Add(x.Key, x.Value));
				Config.ToList().ForEach(x => newConfig.Add(x.Key, x.Value));
				string ConfigJson = Mapper.WriteValueAsString(newConfig);
				return (T)Mapper.ReadValue(ConfigJson, DataCls);
			}
			catch (IOException E)
			{
				throw new System.Exception("Failed to load config into existing configuration data", E);
			}

		}

	}
	
}