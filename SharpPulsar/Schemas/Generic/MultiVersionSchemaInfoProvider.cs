﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Interfaces.ISchema;
using Akka.Event;
using Akka.Actor;
using SharpPulsar.Extension;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Cache;
using System;

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
		private readonly ILoggingAdapter _log;
		private readonly IActorRef _lookup;

		private readonly TopicName _topicName;

		private readonly Cache<BytesSchemaVersion, ISchemaInfo> _cache = new Cache<BytesSchemaVersion, ISchemaInfo>(TimeSpan.FromMinutes(30), 1000);

		
		public MultiVersionSchemaInfoProvider(TopicName topicName, ILoggingAdapter log, IActorRef lookup)
		{
			_topicName = topicName;
			_log = log;
			_lookup = lookup;
		}

		public ISchemaInfo GetSchemaByVersion(sbyte[] schemaVersion)
		{
			try
            {
                if (schemaVersion != null)
                    return _cache.Get(BytesSchemaVersion.Of(schemaVersion));
                return null;

            }
			catch (Exception e)
			{
				_log.Error($"Can't get schema for topic {_topicName} schema version {StringHelper.NewString(schemaVersion)}: {e}");
				return null; 
			}
		}

		public virtual ISchemaInfo LatestSchema
		{
			get
			{
				var sch = _lookup.AskFor<GetSchemaInfoResponse>(new GetSchema(_topicName)).SchemaInfo;
				_cache.Put(BytesSchemaVersion.Of(sch.Schema), sch);
				return sch;
            }
		}

		public virtual string TopicName => _topicName.LocalName;

        private ISchemaInfo LoadSchema(sbyte[] schemaVersion)
		{
			 return null;
		}

	}

}