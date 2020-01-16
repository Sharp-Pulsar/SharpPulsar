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
namespace org.apache.pulsar.client.admin.@internal.http
{

	using ClientConfigurationData = org.apache.pulsar.client.impl.conf.ClientConfigurationData;
	using Connector = org.glassfish.jersey.client.spi.Connector;
	using ConnectorProvider = org.glassfish.jersey.client.spi.ConnectorProvider;

	public class AsyncHttpConnectorProvider : ConnectorProvider
	{

		private readonly ClientConfigurationData conf;

		public AsyncHttpConnectorProvider(ClientConfigurationData conf)
		{
			this.conf = conf;
		}

		public override Connector getConnector(Client client, Configuration runtimeConfig)
		{
			return new AsyncHttpConnector(client, conf);
		}


		public virtual AsyncHttpConnector getConnector(int connectTimeoutMs, int readTimeoutMs, int requestTimeoutMs)
		{
			return new AsyncHttpConnector(connectTimeoutMs, readTimeoutMs, requestTimeoutMs, conf);
		}
	}

}