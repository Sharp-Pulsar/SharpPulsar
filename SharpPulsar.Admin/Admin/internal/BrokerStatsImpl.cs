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
namespace org.apache.pulsar.client.admin.@internal
{

	using Authentication = org.apache.pulsar.client.api.Authentication;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using AllocatorStats = org.apache.pulsar.common.stats.AllocatorStats;
	using LoadManagerReport = org.apache.pulsar.policies.data.loadbalancer.LoadManagerReport;
	using LocalBrokerData = org.apache.pulsar.policies.data.loadbalancer.LocalBrokerData;

	using Gson = com.google.gson.Gson;
	using JsonArray = com.google.gson.JsonArray;
	using JsonObject = com.google.gson.JsonObject;

	/// <summary>
	/// Pulsar Admin API client.
	/// 
	/// 
	/// </summary>
	public class BrokerStatsImpl : BaseResource, BrokerStats
	{

		private readonly WebTarget adminBrokerStats;
		private readonly WebTarget adminV2BrokerStats;

		public BrokerStatsImpl(WebTarget target, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminBrokerStats = target.path("/admin/broker-stats");
			adminV2BrokerStats = target.path("/admin/v2/broker-stats");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonArray getMetrics() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonArray Metrics
		{
			get
			{
				try
				{
					string json = request(adminV2BrokerStats.path("/metrics")).get(typeof(string));
					return (new Gson()).fromJson(json, typeof(JsonArray));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.stats.AllocatorStats getAllocatorStats(String allocatorName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual AllocatorStats getAllocatorStats(string allocatorName)
		{
			try
			{
				return request(adminV2BrokerStats.path("/allocator-stats").path(allocatorName)).get(typeof(AllocatorStats));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonArray getMBeans() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonArray MBeans
		{
			get
			{
				try
				{
					string json = request(adminV2BrokerStats.path("/mbeans")).get(typeof(string));
					return (new Gson()).fromJson(json, typeof(JsonArray));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonObject getTopics() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonObject Topics
		{
			get
			{
				try
				{
					string json = request(adminV2BrokerStats.path("/topics")).get(typeof(string));
					return (new Gson()).fromJson(json, typeof(JsonObject));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.policies.data.loadbalancer.LoadManagerReport getLoadReport() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual LoadManagerReport LoadReport
		{
			get
			{
				try
				{
					return request(adminV2BrokerStats.path("/load-report")).get(typeof(LocalBrokerData));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonObject getPendingBookieOpsStats() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonObject PendingBookieOpsStats
		{
			get
			{
				try
				{
					string json = request(adminV2BrokerStats.path("/bookieops")).get(typeof(string));
					return (new Gson()).fromJson(json, typeof(JsonObject));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public com.google.gson.JsonObject getBrokerResourceAvailability(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonObject getBrokerResourceAvailability(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget admin = ns.V2 ? adminV2BrokerStats : adminBrokerStats;
				string json = request(admin.path("/broker-resource-availability").path(ns.ToString())).get(typeof(string));
				return (new Gson()).fromJson(json, typeof(JsonObject));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}