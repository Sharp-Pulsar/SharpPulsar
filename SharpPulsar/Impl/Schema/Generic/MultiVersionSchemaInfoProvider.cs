using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;

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
namespace SharpPulsar.Impl.Schema.Generic
{


	/// <summary>
	/// Multi version generic schema provider by guava cache.
	/// </summary>
	public class MultiVersionSchemaInfoProvider : ISchemaInfoProvider
	{

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(MultiVersionSchemaInfoProvider));

		private readonly TopicName _topicName;

		private readonly ConcurrentDictionary<BytesSchemaVersion, ISchemaInfo> _cache = new ConcurrentDictionary<BytesSchemaVersion, ISchemaInfo>();

		
		public MultiVersionSchemaInfoProvider(TopicName topicName)
		{
			_topicName = topicName;
		}

		public ISchemaInfo GetSchemaByVersion(sbyte[] schemaVersion)
		{
			try
            {
                if (schemaVersion != null)
                    return _cache[BytesSchemaVersion.Of(schemaVersion)];
                return null;

            }
			catch (System.Exception e)
			{
				Log.LogError("Can't get schema for topic {} schema version {}", _topicName.ToString(), StringHelper.NewString(schemaVersion), e);
				return null; 
			}
		}

		public virtual ISchemaInfo LatestSchema
		{
			get
			{
				var lkup = PulsarClient.Lookup.GetSchema(_topicName);
                return new ValueTask<ISchemaInfo>(lkup.Result);
            }
		}

		public virtual string TopicName => _topicName.LocalName;

        private ValueTask<SchemaInfo> LoadSchema(sbyte[] schemaVersion)
		{
			 return PulsarClient.Lookup.GetSchema(_topicName, schemaVersion);
		}

	}

}