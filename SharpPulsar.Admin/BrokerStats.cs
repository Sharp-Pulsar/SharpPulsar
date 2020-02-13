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
namespace Org.Apache.Pulsar.Client.Admin
{
	using AllocatorStats = Org.Apache.Pulsar.Common.Stats.AllocatorStats;
	using LoadManagerReport = Org.Apache.Pulsar.Policies.Data.Loadbalancer.LoadManagerReport;

	using JsonArray = com.google.gson.JsonArray;
	using JsonObject = com.google.gson.JsonObject;

	/// <summary>
	/// Admin interface for brokers management.
	/// </summary>
	public interface BrokerStats
	{

		/// <summary>
		/// Returns Monitoring metrics
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: com.google.gson.JsonArray getMetrics() throws PulsarAdminException;
		JsonArray Metrics {get;}

		/// <summary>
		/// Requests JSON string server mbean dump
		/// <para>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return
		/// </para>
		/// </summary>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: com.google.gson.JsonArray getMBeans() throws PulsarAdminException;
		JsonArray MBeans {get;}

		/// <summary>
		/// Returns JSON string topics stats
		/// <para>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return
		/// </para>
		/// </summary>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: com.google.gson.JsonObject getTopics() throws PulsarAdminException;
		JsonObject Topics {get;}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: com.google.gson.JsonObject getPendingBookieOpsStats() throws PulsarAdminException;
		JsonObject PendingBookieOpsStats {get;}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.stats.AllocatorStats getAllocatorStats(String allocatorName) throws PulsarAdminException;
		AllocatorStats GetAllocatorStats(string AllocatorName);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.policies.data.loadbalancer.LoadManagerReport getLoadReport() throws PulsarAdminException;
		LoadManagerReport LoadReport {get;}
	}

}