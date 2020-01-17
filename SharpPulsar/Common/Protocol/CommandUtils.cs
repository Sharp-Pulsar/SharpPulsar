using System.Collections.Generic;
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
namespace SharpPulsar.Common.Protocol
{

	/// <summary>
	/// Helper class to work with commands.
	/// </summary>
	public sealed class CommandUtils
	{

		private CommandUtils()
		{
		}

		public static IDictionary<string, string> MetadataFromCommand(Proto.CommandProducer commandProducer)
		{
			return ToMap(commandProducer.MetadataList);
		}

		public static IDictionary<string, string> MetadataFromCommand(Proto.CommandSubscribe commandSubscribe)
		{
			
			return ToMap(commandSubscribe.MetadataList);
		}

		internal static IList<Proto.KeyValue> ToKeyValueList(IDictionary<string, string> metadata)
		{
			if (metadata == null || metadata.Count == 0)
			{
				return Collections.emptyList();
			}

			return metadata.SetOfKeyValuePairs().Select(e => PulsarApi.KeyValue.NewBuilder().setKey(e.Key).setValue(e.Value).build()).ToList();
		}

		private static IDictionary<string, string> ToMap(IList<Proto.KeyValue> keyValues)
		{
			if (keyValues == null || keyValues.Count == 0)
			{
				return Collections.emptyMap();
			}
			return keyValues.ToDictionary(k => k.Key, k => k.Value);
		}
	}

}