using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Table.Messages;
using SharpPulsar.User;

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

namespace SharpPulsar.Table
{
	public class TableViewBuilder<T> : ITableViewBuilder<T>
	{

		private readonly PulsarClient _client;
		private readonly ISchema<T> _schema;
		private TableViewConfigurationData _conf;

		internal TableViewBuilder(PulsarClient client, ISchema<T> schema)
		{
			_client = client;
			_schema = schema;
			_conf = new TableViewConfigurationData();
		}

		public virtual ITableViewBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			_conf = (TableViewConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
			return this;
		}
        public virtual ITableView<T> Create()
		{
		   try
		   {
			   return CreateAsync().GetAwaiter().GetResult();
		   }
		   catch (Exception e)
		   {
			   throw PulsarClientException.Unwrap(e);
		   }
		}

		public virtual async ValueTask<ITableView<T>> CreateAsync()
		{
            var data = new ConcurrentDictionary<string, T>();
            var actor = _client.ActorSystem.ActorOf(TableViewActor<T>.Prop(_client, _schema, _conf, data));
             await Task.Delay(TimeSpan.FromSeconds(5));
            var response = await actor.Ask<AskResponse>(StartMessage.Instance);
            if (response.Failed)
            {
                await actor.GracefulStop(TimeSpan.FromSeconds(1));
                throw response.Exception;
            }
            return new TableView<T>(actor, data);
		}

		public virtual ITableViewBuilder<T> Topic(string topic)
		{
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentNullException("topic cannot be blank");
            }
            _conf.TopicName = topic.Trim();
		   return this;
		}

		public virtual ITableViewBuilder<T> AutoUpdatePartitionsInterval(TimeSpan interval)
		{
		   if(interval.TotalSeconds < 1)
            {
                throw new ArgumentNullException("minimum is 1 second");
            }
		   _conf.AutoUpdatePartitionsSeconds = interval;
		   return this;
		}
	}

}