using System.Collections.Generic;
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
    /// Admin interface for worker stats management.
    /// </summary>
    public interface IWorker
	{

		/// <summary>
		/// Get all functions stats on a worker.
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.List<org.apache.pulsar.common.policies.data.WorkerFunctionInstanceStats> getFunctionsStats() throws PulsarAdminException;
		IList<WorkerFunctionInstanceStats> FunctionsStats {get;}

		/// <summary>
		/// Get all functions stats on a worker asynchronously.
		/// @return
		/// </summary>
		ValueTask<IList<WorkerFunctionInstanceStats>> FunctionsStatsAsync {get;}

		/// <summary>
		/// Get worker metrics.
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.Collection<org.apache.pulsar.common.stats.Metrics> getMetrics() throws PulsarAdminException;
		ICollection<Metrics> Metrics {get;}

		/// <summary>
		/// Get worker metrics asynchronously.
		/// @return
		/// </summary>
		ValueTask<ICollection<Metrics>> MetricsAsync {get;}

		/// <summary>
		/// Get List of all workers belonging to this cluster.
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.List<org.apache.pulsar.common.functions.WorkerInfo> getCluster() throws PulsarAdminException;
		IList<WorkerInfo> Cluster {get;}

		/// <summary>
		/// Get List of all workers belonging to this cluster asynchronously.
		/// @return
		/// </summary>
		ValueTask<IList<WorkerInfo>> ClusterAsync {get;}

		/// <summary>
		/// Get the worker who is the leader of the cluster.
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.functions.WorkerInfo getClusterLeader() throws PulsarAdminException;
		WorkerInfo ClusterLeader {get;}

		/// <summary>
		/// Get the worker who is the leader of the cluster asynchronously.
		/// @return
		/// </summary>
		ValueTask<WorkerInfo> ClusterLeaderAsync {get;}

		/// <summary>
		/// Get the function assignment among the cluster.
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.Map<String, java.util.Collection<String>> getAssignments() throws PulsarAdminException;
		IDictionary<string, ICollection<string>> Assignments {get;}

		/// <summary>
		/// Get the function assignment among the cluster asynchronously.
		/// @return
		/// </summary>
		ValueTask<IDictionary<string, ICollection<string>>> AssignmentsAsync {get;}

		/// <summary>
		/// Triggers a rebalance of functions to workers. </summary>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void rebalance() throws PulsarAdminException;
		void Rebalance();

		/// <summary>
		/// Triggers a rebalance of functions to workersasynchronously.. </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		ValueTask RebalanceAsync();
	}

}