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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{


	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using BrokerNamespaceIsolationData = Org.Apache.Pulsar.Common.Policies.Data.BrokerNamespaceIsolationData;
	using ClusterData = Org.Apache.Pulsar.Common.Policies.Data.ClusterData;
	using FailureDomain = Org.Apache.Pulsar.Common.Policies.Data.FailureDomain;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using NamespaceIsolationData = Org.Apache.Pulsar.Common.Policies.Data.NamespaceIsolationData;

	public class ClustersImpl : BaseResource, Clusters
	{

		private readonly WebTarget adminClusters;

		public ClustersImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminClusters = Web.path("/admin/v2/clusters");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getClusters() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> Clusters
		{
			get
			{
				try
				{
					return Request(adminClusters).get(new GenericTypeAnonymousInnerClass(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass(ClustersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.ClusterData getCluster(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override ClusterData GetCluster(string Cluster)
		{
			try
			{
				return Request(adminClusters.path(Cluster)).get(typeof(ClusterData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateCluster(string Cluster, ClusterData ClusterData)
		{
			try
			{
				Request(adminClusters.path(Cluster)).put(Entity.entity(ClusterData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateCluster(string Cluster, ClusterData ClusterData)
		{
			try
			{
				Request(adminClusters.path(Cluster)).post(Entity.entity(ClusterData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updatePeerClusterNames(String cluster, java.util.LinkedHashSet<String> peerClusterNames) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdatePeerClusterNames(string Cluster, LinkedHashSet<string> PeerClusterNames)
		{
			try
			{
				Request(adminClusters.path(Cluster).path("peers")).post(Entity.entity(PeerClusterNames, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public java.util.Set<String> getPeerClusterNames(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public override ISet<string> GetPeerClusterNames(string Cluster)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("peers")).get(typeof(LinkedHashSet));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteCluster(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteCluster(string Cluster)
		{
			try
			{
				Request(adminClusters.path(Cluster)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceIsolationData> getNamespaceIsolationPolicies(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<string, NamespaceIsolationData> GetNamespaceIsolationPolicies(string Cluster)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies")).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass2 : GenericType<IDictionary<string, NamespaceIsolationData>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(ClustersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<BrokerNamespaceIsolationData> GetBrokersWithNamespaceIsolationPolicy(string Cluster)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies").path("brokers")).get(new GenericTypeAnonymousInnerClass3(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass3 : GenericType<IList<BrokerNamespaceIsolationData>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(ClustersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(String cluster, String broker) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override BrokerNamespaceIsolationData GetBrokerWithNamespaceIsolationPolicy(string Cluster, string Broker)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies").path("brokers").path(Broker)).get(typeof(BrokerNamespaceIsolationData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespaceIsolationPolicy(string Cluster, string PolicyName, NamespaceIsolationData NamespaceIsolationData)
		{
			SetNamespaceIsolationPolicy(Cluster, PolicyName, NamespaceIsolationData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateNamespaceIsolationPolicy(string Cluster, string PolicyName, NamespaceIsolationData NamespaceIsolationData)
		{
			SetNamespaceIsolationPolicy(Cluster, PolicyName, NamespaceIsolationData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceIsolationPolicy(String cluster, String policyName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteNamespaceIsolationPolicy(string Cluster, string PolicyName)
		{
			Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies").path(PolicyName)).delete(typeof(ErrorData));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void SetNamespaceIsolationPolicy(string Cluster, string PolicyName, NamespaceIsolationData NamespaceIsolationData)
		{
			try
			{
				Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies").path(PolicyName)).post(Entity.entity(NamespaceIsolationData, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.NamespaceIsolationData getNamespaceIsolationPolicy(String cluster, String policyName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override NamespaceIsolationData GetNamespaceIsolationPolicy(string Cluster, string PolicyName)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("namespaceIsolationPolicies").path(PolicyName)).get(typeof(NamespaceIsolationData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateFailureDomain(string Cluster, string DomainName, FailureDomain Domain)
		{
			SetDomain(Cluster, DomainName, Domain);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateFailureDomain(string Cluster, string DomainName, FailureDomain Domain)
		{
			SetDomain(Cluster, DomainName, Domain);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteFailureDomain(String cluster, String domainName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteFailureDomain(string Cluster, string DomainName)
		{
			Request(adminClusters.path(Cluster).path("failureDomains").path(DomainName)).delete(typeof(ErrorData));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.FailureDomain> getFailureDomains(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<string, FailureDomain> GetFailureDomains(string Cluster)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("failureDomains")).get(new GenericTypeAnonymousInnerClass4(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass4 : GenericType<IDictionary<string, FailureDomain>>
		{
			private readonly ClustersImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(ClustersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FailureDomain getFailureDomain(String cluster, String domainName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FailureDomain GetFailureDomain(string Cluster, string DomainName)
		{
			try
			{
				return Request(adminClusters.path(Cluster).path("failureDomains").path(DomainName)).get(typeof(FailureDomain));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void SetDomain(string Cluster, string DomainName, FailureDomain Domain)
		{
			try
			{
				Request(adminClusters.path(Cluster).path("failureDomains").path(DomainName)).post(Entity.entity(Domain, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}
	}

}