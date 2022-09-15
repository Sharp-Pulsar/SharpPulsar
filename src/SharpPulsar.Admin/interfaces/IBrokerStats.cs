
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin.Models;

namespace SharpPulsar.Admin.interfaces
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

        IList<Metrics> GetMetrics(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Returns Monitoring metrics asynchronously.
        /// 
        /// @return
        /// </summary>

        ValueTask<IList<Metrics>> GetMetricsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Requests JSON string server mbean dump.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return </summary>
        IList<Metrics> GetMBeans(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Requests JSON string server mbean dump asynchronously.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return
        /// </summary>
        ValueTask<IList<Metrics>> GetMBeansAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns JSON string topics stats.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return </summary>
        object GetTopics(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Returns JSON string topics stats asynchronously.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return
        /// </summary>
        ValueTask<object> GetTopicsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Get pending bookie client op stats by namespace.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return </summary>
        IDictionary<string, PendingBookieOpsStats> GetPendingBookieOpsStats(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get pending bookie client op stats by namespace asynchronously.
        /// <p/>
        /// Notes: since we don't plan to introspect the response we avoid converting the response into POJO.
        /// 
        /// @return
        /// </summary>
        ValueTask<IDictionary<string, PendingBookieOpsStats>> GetPendingBookieOpsStatsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the stats for the Netty allocator.
        /// </summary>
        /// <param name="allocatorName">
        /// @return </param>
        AllocatorStats GetAllocatorStats(string allocatorName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the stats for the Netty allocator asynchronously.
        /// </summary>
        /// <param name="allocatorName">
        /// @return </param>
        ValueTask<AllocatorStats> GetAllocatorStatsAsync(string allocatorName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get load for this broker.
        /// 
        /// @return </summary>
        LoadReport GetLoadReport(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get load for this broker asynchronously.
        /// 
        /// @return
        /// </summary>
        ValueTask<LoadReport> GetLoadReportAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        IDictionary<string, ResourceUnit> GetBrokerResourceAvailability(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null);
        ValueTask<IDictionary<string, ResourceUnit>> GetBrokerResourceAvailabilityAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
