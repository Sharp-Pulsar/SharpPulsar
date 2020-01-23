using System;
using System.Collections.Generic;

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
	using Include = com.fasterxml.jackson.annotation.JsonInclude.Include;
	using DeserializationFeature = com.fasterxml.jackson.databind.DeserializationFeature;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using Maps = com.google.common.collect.Maps;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;

	/// <summary>
	/// Utils for loading configuration data.
	/// </summary>
	public sealed class ConfigurationDataUtils
	{

		public static ObjectMapper Create()
		{
			ObjectMapper Mapper = new ObjectMapper();
			// forward compatibility for the properties may go away in the future
			Mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, true);
			Mapper.configure(DeserializationFeature.READ_UNKNOWN_ENUM_VALUES_AS_NULL, false);
			Mapper.SerializationInclusion = Include.NON_NULL;
			return Mapper;
		}

		private static readonly FastThreadLocal<ObjectMapper> mapper = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected com.fasterxml.jackson.databind.ObjectMapper initialValue() throws Exception
			public override ObjectMapper initialValue()
			{
				return Create();
			}
		}

		public static ObjectMapper ThreadLocal
		{
			get
			{
				return mapper.get();
			}
		}

		private ConfigurationDataUtils()
		{
		}

		public static T LoadData<T>(IDictionary<string, object> Config, T ExistingData, Type DataCls)
		{
			ObjectMapper Mapper = ThreadLocal;
			try
			{
				string ExistingConfigJson = Mapper.writeValueAsString(ExistingData);
				IDictionary<string, object> ExistingConfig = Mapper.readValue(ExistingConfigJson, typeof(System.Collections.IDictionary));
				IDictionary<string, object> NewConfig = Maps.newHashMap();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				NewConfig.putAll(ExistingConfig);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				NewConfig.putAll(Config);
				string ConfigJson = Mapper.writeValueAsString(NewConfig);
				return Mapper.readValue(ConfigJson, DataCls);
			}
			catch (IOException E)
			{
				throw new Exception("Failed to load config into existing configuration data", E);
			}

		}

	}


}