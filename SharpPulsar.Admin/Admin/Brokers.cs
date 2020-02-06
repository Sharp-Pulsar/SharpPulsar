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
namespace org.apache.pulsar.client.admin
{

	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using InternalConfigurationData = pulsar.common.conf.InternalConfigurationData;
	using NamespaceOwnershipStatus = pulsar.common.policies.data.NamespaceOwnershipStatus;

	/// <summary>
	/// Admin interface for brokers management.
	/// </summary>
	public interface Brokers
	{
		/// <summary>
		/// Get the list of active brokers in the cluster.
		/// <para>
		/// Get the list of active brokers (web service addresses) in the cluster.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["prod1-broker1.messaging.use.example.com:8080", "prod1-broker2.messaging.use.example.com:8080", "prod1-broker3.messaging.use.example.com:8080"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <returns> a list of (host:port) </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the list of active brokers in the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getActiveBrokers(String cluster) throws PulsarAdminException;
		IList<string> getActiveBrokers(string cluster);


		/// <summary>
		/// Get the map of owned namespaces and their status from a single broker in the cluster
		/// <para>
		/// The map is returned in a JSON object format below
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{"ns-1":{"broker_assignment":"shared","is_active":"true","is_controlled":"false"}, "ns-2":{"broker_assignment":"primary","is_active":"true","is_controlled":"true"}}</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="brokerUrl">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceOwnershipStatus> getOwnedNamespaces(String cluster, String brokerUrl) throws PulsarAdminException;
		IDictionary<string, NamespaceOwnershipStatus> getOwnedNamespaces(string cluster, string brokerUrl);

		/// <summary>
		/// It updates dynamic configuration value in to Zk that triggers watch on
		/// brokers and all brokers can update <seealso cref="ServiceConfiguration"/> value
		/// locally
		/// </summary>
		/// <param name="key"> </param>
		/// <param name="value"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateDynamicConfiguration(String configName, String configValue) throws PulsarAdminException;
		void updateDynamicConfiguration(string configName, string configValue);

		/// <summary>
		/// It deletes dynamic configuration value in to Zk. It will not impact current value in broker but next time when
		/// broker restarts, it applies value from configuration file only.
		/// </summary>
		/// <param name="key"> </param>
		/// <param name="value"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteDynamicConfiguration(String configName) throws PulsarAdminException;
		void deleteDynamicConfiguration(string configName);

		/// <summary>
		/// Get list of updatable configuration name
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getDynamicConfigurationNames() throws PulsarAdminException;
		IList<string> DynamicConfigurationNames {get;}

		/// <summary>
		/// Get values of runtime configuration
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, String> getRuntimeConfigurations() throws PulsarAdminException;
		IDictionary<string, string> RuntimeConfigurations {get;}

		/// <summary>
		/// Get values of all overridden dynamic-configs
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, String> getAllDynamicConfigurations() throws PulsarAdminException;
		IDictionary<string, string> AllDynamicConfigurations {get;}

		/// <summary>
		/// Get the internal configuration data.
		/// </summary>
		/// <returns> internal configuration data. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.conf.InternalConfigurationData getInternalConfigurationData() throws PulsarAdminException;
		InternalConfigurationData InternalConfigurationData {get;}

		/// <summary>
		/// Run a healthcheck on the broker.
		/// </summary>
		/// <exception cref="an"> exception if the healthcheck fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void healthcheck() throws PulsarAdminException;
		void healthcheck();
	}

}