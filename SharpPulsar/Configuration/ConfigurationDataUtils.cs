using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SharpPulsar.Utils;
using SharpPulsar.Interfaces;
using SharpPulsar.Table;

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
namespace SharpPulsar.Configuration
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

        public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
			public ObjectMapper InitialValue()
			{
				return Create();
			}
		}

		public static ObjectMapper ThreadLocal { get; } = new FastThreadLocalAnonymousInnerClass().InitialValue();

        private ConfigurationDataUtils()
		{
		}

		public static object LoadData(IDictionary<string, object> config, object existingData)
		{
			var mapper = ThreadLocal;
			try
			{
                var existingConfigJson = JsonSerializer.Serialize(existingData);
				var existingConfig = (Dictionary<string, object>)mapper.ReadValue(existingConfigJson, typeof(Dictionary<string, object>));
				IDictionary<string, object> newConfig = new Dictionary<string, object>();
				existingConfig.ToList().ForEach(x=> newConfig[x.Key] = x.Value);
				config.ToList().ForEach(x => newConfig[x.Key] = x.Value);
				var configJson = JsonSerializer.Serialize(newConfig);
                var fullName = existingData.GetType().Name;
                if (fullName != null)
                {
                    if (fullName.Contains("ProducerConfigurationData"))
                    {
                        return (ProducerConfigurationData)mapper.ReadValue(configJson, typeof(ProducerConfigurationData));
                    }
					else if (fullName.Contains("ClientConfigurationData"))
                    {
                        return (ClientConfigurationData)mapper.ReadValue(configJson, typeof(ClientConfigurationData), ClientConfigurationDataOptions(JsonSerializer.Serialize(newConfig["Authentication"]), mapper));
					}
					else if (fullName.Contains("TableViewConfigurationData"))
                    {
                        return (TableViewConfigurationData)mapper.ReadValue(configJson, typeof(TableViewConfigurationData));
					}
                    else
                    {
						return (ClientConfigurationData)mapper.ReadValue(configJson, typeof(ClientConfigurationData));
					}
                }
                throw new NullReferenceException("ConfigurationData is null");
			}
			catch (IOException e)
			{
				throw new Exception("Failed to load config into existing configuration data", e);
			}

		}

        private static JsonSerializerOptions ClientConfigurationDataOptions(string auth, ObjectMapper mapper)
        {
            var authObj = (Dictionary<string, object>)mapper.ReadValue(auth, typeof(Dictionary<string, object>));
            var authMethod = authObj["AuthMethodName"].ToString();

			var iuath = authMethod switch
            {
                "none" => typeof(Auth.AuthenticationDisabled),
                "tls" => typeof(Auth.AuthenticationTls),
                "token" => typeof(Auth.AuthenticationToken),
                "basic" => typeof(Auth.AuthenticationBasic),
                _ => null
            };
            if(iuath == null)
				throw new NullReferenceException("Authentication is null");
			var serializerOptions = new JsonSerializerOptions
            {
                Converters = {
                    new InterfaceConverterFactory(iuath, typeof(IAuthentication))
                },
                ReadCommentHandling = JsonCommentHandling.Skip,
				PropertyNameCaseInsensitive = true
			};
            return serializerOptions;
        }
    }

}