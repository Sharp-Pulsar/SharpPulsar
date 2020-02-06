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
namespace org.apache.pulsar.client.admin.@internal
{


	using Authentication = client.api.Authentication;
	using BrokerNamespaceIsolationData = pulsar.common.policies.data.BrokerNamespaceIsolationData;
	using ClusterData = pulsar.common.policies.data.ClusterData;
	using FailureDomain = pulsar.common.policies.data.FailureDomain;
	using ErrorData = pulsar.common.policies.data.ErrorData;
	using NamespaceIsolationData = pulsar.common.policies.data.NamespaceIsolationData;

	public class ClustersImpl : BaseResource, Clusters
	{

		private readonly WebTarget adminClusters;

		public ClustersImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminClusters = web.path("/admin/v2/clusters");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getClusters() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> Clusters
		{
			get
			{
				try
				{
					return request(adminClusters).get(new GenericTypeAnonymousInnerClass(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass(ClustersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.ClusterData getCluster(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ClusterData getCluster(string cluster)
		{
			try
			{
				return request(adminClusters.path(cluster)).get(typeof(ClusterData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createCluster(string cluster, ClusterData clusterData)
		{
			try
			{
				request(adminClusters.path(cluster)).put(Entity.entity(clusterData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateCluster(string cluster, ClusterData clusterData)
		{
			try
			{
				request(adminClusters.path(cluster)).post(Entity.entity(clusterData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updatePeerClusterNames(String cluster, java.util.LinkedHashSet<String> peerClusterNames) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updatePeerClusterNames(string cluster, LinkedHashSet<string> peerClusterNames)
		{
			try
			{
				request(adminClusters.path(cluster).path("peers")).post(Entity.entity(peerClusterNames, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public java.util.Set<String> getPeerClusterNames(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual ISet<string> getPeerClusterNames(string cluster)
		{
			try
			{
				return request(adminClusters.path(cluster).path("peers")).get(typeof(LinkedHashSet));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteCluster(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteCluster(string cluster)
		{
			try
			{
				request(adminClusters.path(cluster)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceIsolationData> getNamespaceIsolationPolicies(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, NamespaceIsolationData> getNamespaceIsolationPolicies(string cluster)
		{
			try
			{
				return request(adminClusters.path(cluster).path("namespaceIsolationPolicies")).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IDictionary<string, NamespaceIsolationData>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(ClustersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(string cluster)
		{
			try
			{
				return request(adminClusters.path(cluster).path("namespaceIsolationPolicies").path("brokers")).get(new GenericTypeAnonymousInnerClass3(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IList<BrokerNamespaceIsolationData>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(ClustersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(String cluster, String broker) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(string cluster, string broker)
		{
			try
			{
				return request(adminClusters.path(cluster).path("namespaceIsolationPolicies").path("brokers").path(broker)).get(typeof(BrokerNamespaceIsolationData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData)
		{
			setNamespaceIsolationPolicy(cluster, policyName, namespaceIsolationData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData)
		{
			setNamespaceIsolationPolicy(cluster, policyName, namespaceIsolationData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceIsolationPolicy(String cluster, String policyName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteNamespaceIsolationPolicy(string cluster, string policyName)
		{
			request(adminClusters.path(cluster).path("namespaceIsolationPolicies").path(policyName)).delete(typeof(ErrorData));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void setNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData)
		{
			try
			{
				request(adminClusters.path(cluster).path("namespaceIsolationPolicies").path(policyName)).post(Entity.entity(namespaceIsolationData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.NamespaceIsolationData getNamespaceIsolationPolicy(String cluster, String policyName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual NamespaceIsolationData getNamespaceIsolationPolicy(string cluster, string policyName)
		{
			try
			{
				return request(adminClusters.path(cluster).path("namespaceIsolationPolicies").path(policyName)).get(typeof(NamespaceIsolationData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createFailureDomain(string cluster, string domainName, FailureDomain domain)
		{
			setDomain(cluster, domainName, domain);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateFailureDomain(string cluster, string domainName, FailureDomain domain)
		{
			setDomain(cluster, domainName, domain);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteFailureDomain(String cluster, String domainName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteFailureDomain(string cluster, string domainName)
		{
			request(adminClusters.path(cluster).path("failureDomains").path(domainName)).delete(typeof(ErrorData));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.FailureDomain> getFailureDomains(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, FailureDomain> getFailureDomains(string cluster)
		{
			try
			{
				return request(adminClusters.path(cluster).path("failureDomains")).get(new GenericTypeAnonymousInnerClass4(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass4 : GenericType<IDictionary<string, FailureDomain>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(ClustersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FailureDomain getFailureDomain(String cluster, String domainName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FailureDomain getFailureDomain(string cluster, string domainName)
		{
			try
			{
				return request(adminClusters.path(cluster).path("failureDomains").path(domainName)).get(typeof(FailureDomain));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void setDomain(string cluster, string domainName, FailureDomain domain)
		{
			try
			{
				request(adminClusters.path(cluster).path("failureDomains").path(domainName)).post(Entity.entity(domain, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}