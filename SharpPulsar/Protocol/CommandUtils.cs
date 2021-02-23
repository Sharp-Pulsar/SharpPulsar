using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Extension;
using HashMapHelper = SharpPulsar.Presto.HashMapHelper;

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
namespace SharpPulsar.Protocol
{
	/// <summary>
	/// Helper class to work with new Commands().
	/// </summary>
	public sealed class CommandUtils
	{

		private CommandUtils()
		{
		}

		public static IDictionary<string, string> MetadataFromCommand(CommandProducer commandProducer)
		{
			return ToMap(commandProducer.Metadatas);
		}

		public static IDictionary<string, string> MetadataFromCommand(CommandSubscribe commandSubscribe)
		{
			return ToMap(commandSubscribe.Metadatas);
		}

		internal static IList<KeyValue> ToKeyValueList(IDictionary<string, string> metadata)
		{
			if (metadata == null || metadata.Count == 0)
			{
				return Array.Empty<KeyValue>();
			}

			return HashMapHelper.SetOfKeyValuePairs(metadata).Select(e => new KeyValueBuilder().SetKey(e.Key).SetValue(e.Value).Build()).ToList();
		}

		private static IDictionary<string, string> ToMap(IList<KeyValue> keyValues)
		{
			if (keyValues == null || keyValues.Count == 0)
			{
				return new Dictionary<string,string>();
			}

			return keyValues.ToDictionary(k => k.Key, k => k.Value);
		}
	}

}