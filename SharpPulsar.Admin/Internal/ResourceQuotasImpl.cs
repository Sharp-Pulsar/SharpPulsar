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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{

	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using ResourceQuota = Org.Apache.Pulsar.Common.Policies.Data.ResourceQuota;

	public class ResourceQuotasImpl : BaseResource, ResourceQuotas
	{

		private readonly WebTarget adminQuotas;
		private readonly WebTarget adminV2Quotas;

		public ResourceQuotasImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminQuotas = Web.path("/admin/resource-quotas");
			adminV2Quotas = Web.path("/admin/v2/resource-quotas");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.ResourceQuota getDefaultResourceQuota() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ResourceQuota DefaultResourceQuota
		{
			get
			{
				try
				{
					return Request(adminV2Quotas).get(typeof(ResourceQuota));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
			set
			{
				try
				{
					Request(adminV2Quotas).post(Entity.entity(value, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.ResourceQuota getNamespaceBundleResourceQuota(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ResourceQuota GetNamespaceBundleResourceQuota(string Namespace, string Bundle)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, Bundle);
				return Request(Path).get(typeof(ResourceQuota));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setNamespaceBundleResourceQuota(String namespace, String bundle, org.apache.pulsar.common.policies.data.ResourceQuota quota) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void SetNamespaceBundleResourceQuota(string Namespace, string Bundle, ResourceQuota Quota)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, Bundle);
				Request(Path).post(Entity.entity(Quota, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void resetNamespaceBundleResourceQuota(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void ResetNamespaceBundleResourceQuota(string Namespace, string Bundle)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, Bundle);
				Request(Path).delete();
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		private WebTarget NamespacePath(NamespaceName Namespace, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Quotas : adminQuotas;
			WebTarget Base = Namespace.V2 ? adminV2Quotas : adminQuotas;
			WebTarget NamespacePath = Base.path(Namespace.ToString());
			NamespacePath = WebTargets.AddParts(NamespacePath, Parts);
			return NamespacePath;
		}
	}


}