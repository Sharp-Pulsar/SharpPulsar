using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
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

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(MultiVersionSchemaInfoProvider));

		private readonly TopicName _topicName;
		public virtual PulsarClientImpl PulsarClient { get;}

		private readonly ConcurrentDictionary<BytesSchemaVersion, Task<ISchemaInfo>> _cache = new ConcurrentDictionary<BytesSchemaVersion, Task<ISchemaInfo>>();

		
		public MultiVersionSchemaInfoProvider(TopicName topicName, PulsarClientImpl pulsarClient)
		{
			_topicName = topicName;
			PulsarClient = pulsarClient;
		}

		public ValueTask<ISchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
		{
			try
            {
                return null == schemaVersion ? new ValueTask<ISchemaInfo>(Task.FromResult<ISchemaInfo>(null)) : new ValueTask<ISchemaInfo>(_cache[BytesSchemaVersion.Of(schemaVersion)]);
            }
			catch (System.Exception e)
			{
				Log.LogError("Can't get schema for topic {} schema version {}", _topicName.ToString(), StringHelper.NewString(schemaVersion), e);
				return new ValueTask<ISchemaInfo>(Task.FromException<ISchemaInfo>(e)); 
			}
		}

		public virtual ValueTask<ISchemaInfo> LatestSchema
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