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

namespace Org.Apache.Pulsar.Client.Impl
{
// JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
// 	import static com.google.common.@base.Preconditions.checkArgument;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using Org.Apache.Pulsar.Client.Api;
	using Org.Apache.Pulsar.Client.Api;
	using Org.Apache.Pulsar.Client.Api;
	using ConfigurationDataUtils = Org.Apache.Pulsar.Client.Impl.Conf.ConfigurationDataUtils;

	public class TableViewBuilder<T> : TableViewBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private readonly Schema<T> schema;
		private TableViewConfigurationData conf;

		internal TableViewBuilder(PulsarClientImpl Client, Schema<T> Schema)
		{
			this.client = Client;
			this.schema = Schema;
			this.conf = new TableViewConfigurationData();
		}

		public virtual TableViewBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			conf = ConfigurationDataUtils.LoadData(Config, conf, typeof(TableViewConfigurationData));
			return this;
		}

// JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
// ORIGINAL LINE: @Override public org.apache.pulsar.client.api.TableView<T> create() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual TableView<T> Create()
		{
		   try
		   {
			   return CreateAsync().get();
		   }
		   catch (Exception E)
		   {
			   throw PulsarClientException.Unwrap(E);
		   }
		}

		public virtual CompletableFuture<TableView<T>> CreateAsync()
		{
		   return (new TableViewImpl<TableView<T>>(client, schema, conf)).Start();
		}

		public virtual TableViewBuilder<T> Topic(string Topic)
		{
		   checkArgument(StringUtils.isNotBlank(Topic), "topic cannot be blank");
		   conf.setTopicName(StringUtils.Trim(Topic));
		   return this;
		}

		public virtual TableViewBuilder<T> AutoUpdatePartitionsInterval(int Interval, TimeUnit Unit)
		{
		   checkArgument(Unit.toSeconds(Interval) >= 1, "minimum is 1 second");
		   conf.setAutoUpdatePartitionsSeconds(Unit.toSeconds(Interval));
		   return this;
		}
	}

}