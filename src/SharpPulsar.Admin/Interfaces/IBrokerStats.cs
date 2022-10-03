using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
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
namespace SharpPulsar.Admin.Interfaces
{
    /// <summary>
    /// Admin interface for brokers management.
    /// </summary>
    public interface IBrokerStats
	{

		/// <summary>
		/// Returns Monitoring metrics.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		string Metrics {get;}

		/// <summary>
		/// Returns Monitoring metrics asynchronously.
		/// 
		/// @return
		/// </summary>

		ValueTask<string> MetricsAsync {get;}

		/// <summary>
		/// Requests JSON string server mbean dump.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		string MBeans {get;}

		/// <summary>
		/// Requests JSON string server mbean dump asynchronously.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return
		/// </summary>
		ValueTask<string> MBeansAsync {get;}

		/// <summary>
		/// Returns JSON string topics stats.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		string Topics {get;}

		/// <summary>
		/// Returns JSON string topics stats asynchronously.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return
		/// </summary>
		ValueTask<string> TopicsAsync {get;}

		/// <summary>
		/// Get pending bookie client op stats by namespace.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		string PendingBookieOpsStats {get;}

		/// <summary>
		/// Get pending bookie client op stats by namespace asynchronously.
		/// <p/>
		/// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
		/// 
		/// @return
		/// </summary>
		ValueTask<string> PendingBookieOpsStatsAsync {get;}

		/// <summary>
		/// Get the stats for the Netty allocator.
		/// </summary>
		/// <param name="allocatorName">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		AllocatorStats GetAllocatorStats(string allocatorName);

		/// <summary>
		/// Get the stats for the Netty allocator asynchronously.
		/// </summary>
		/// <param name="allocatorName">
		/// @return </param>
		ValueTask<AllocatorStats> GetAllocatorStatsAsync(string allocatorName);

		/// <summary>
		/// Get load for this broker.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		LoadReport LoadReport {get;}

		/// <summary>
		/// Get load for this broker asynchronously.
		/// 
		/// @return
		/// </summary>
		ValueTask<LoadReport> LoadReportAsync {get;}
	}

}