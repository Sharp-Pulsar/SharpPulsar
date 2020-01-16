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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using WorkerInfo = org.apache.pulsar.common.functions.WorkerInfo;
	using WorkerFunctionInstanceStats = org.apache.pulsar.common.policies.data.WorkerFunctionInstanceStats;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class WorkerImpl extends BaseResource implements org.apache.pulsar.client.admin.Worker
	public class WorkerImpl : BaseResource, Worker
	{

		private readonly WebTarget workerStats;
		private readonly WebTarget worker;

		public WorkerImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.worker = web.path("/admin/v2/worker");
			this.workerStats = web.path("/admin/v2/worker-stats");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.policies.data.WorkerFunctionInstanceStats> getFunctionsStats() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<WorkerFunctionInstanceStats> FunctionsStats
		{
			get
			{
				try
				{
					Response response = request(workerStats.path("functionsmetrics")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw new ClientErrorException(response);
					}
					IList<WorkerFunctionInstanceStats> metricsList = response.readEntity(new GenericTypeAnonymousInnerClass(this));
					return metricsList;
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<WorkerFunctionInstanceStats>>
		{
			private readonly WorkerImpl outerInstance;

			public GenericTypeAnonymousInnerClass(WorkerImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Collection<org.apache.pulsar.common.stats.Metrics> getMetrics() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ICollection<org.apache.pulsar.common.stats.Metrics> Metrics
		{
			get
			{
				try
				{
					Response response = request(workerStats.path("metrics")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw new ClientErrorException(response);
					}
					return response.readEntity(new GenericTypeAnonymousInnerClass2(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IList<org.apache.pulsar.common.stats.Metrics>>
		{
			private readonly WorkerImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(WorkerImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.functions.WorkerInfo> getCluster() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<WorkerInfo> Cluster
		{
			get
			{
				try
				{
					Response response = request(worker.path("cluster")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw new ClientErrorException(response);
					}
					return response.readEntity(new GenericTypeAnonymousInnerClass3(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IList<WorkerInfo>>
		{
			private readonly WorkerImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(WorkerImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.functions.WorkerInfo getClusterLeader() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual WorkerInfo ClusterLeader
		{
			get
			{
				try
				{
					Response response = request(worker.path("cluster").path("leader")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw new ClientErrorException(response);
					}
					return response.readEntity(new GenericTypeAnonymousInnerClass4(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass4 : GenericType<WorkerInfo>
		{
			private readonly WorkerImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(WorkerImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.Collection<String>> getAssignments() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, ICollection<string>> Assignments
		{
			get
			{
				try
				{
					Response response = request(worker.path("assignments")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw new ClientErrorException(response);
					}
					IDictionary<string, ICollection<string>> assignments = response.readEntity(new GenericTypeAnonymousInnerClass5(this));
					return assignments;
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass5 : GenericType<IDictionary<string, ICollection<string>>>
		{
			private readonly WorkerImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(WorkerImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}
	}
}