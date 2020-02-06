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

	using Authentication = client.api.Authentication;
	using NamespaceName = pulsar.common.naming.NamespaceName;
	using ErrorData = pulsar.common.policies.data.ErrorData;
	using ResourceQuota = pulsar.common.policies.data.ResourceQuota;

	public class ResourceQuotasImpl : BaseResource, ResourceQuotas
	{

		private readonly WebTarget adminQuotas;
		private readonly WebTarget adminV2Quotas;

		public ResourceQuotasImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminQuotas = web.path("/admin/resource-quotas");
			adminV2Quotas = web.path("/admin/v2/resource-quotas");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.ResourceQuota getDefaultResourceQuota() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ResourceQuota DefaultResourceQuota
		{
			get
			{
				try
				{
					return request(adminV2Quotas).get(typeof(ResourceQuota));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
			set
			{
				try
				{
					request(adminV2Quotas).post(Entity.entity(value, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.ResourceQuota getNamespaceBundleResourceQuota(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ResourceQuota getNamespaceBundleResourceQuota(string @namespace, string bundle)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, bundle);
				return request(path).get(typeof(ResourceQuota));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setNamespaceBundleResourceQuota(String namespace, String bundle, org.apache.pulsar.common.policies.data.ResourceQuota quota) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setNamespaceBundleResourceQuota(string @namespace, string bundle, ResourceQuota quota)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, bundle);
				request(path).post(Entity.entity(quota, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void resetNamespaceBundleResourceQuota(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void resetNamespaceBundleResourceQuota(string @namespace, string bundle)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, bundle);
				request(path).delete();
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private WebTarget namespacePath(NamespaceName @namespace, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Quotas : adminQuotas;
			WebTarget @base = @namespace.V2 ? adminV2Quotas : adminQuotas;
			WebTarget namespacePath = @base.path(@namespace.ToString());
			namespacePath = WebTargets.addParts(namespacePath, parts);
			return namespacePath;
		}
	}


}