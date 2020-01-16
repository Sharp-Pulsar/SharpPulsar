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


	using Authentication = org.apache.pulsar.client.api.Authentication;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using TenantInfo = org.apache.pulsar.common.policies.data.TenantInfo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public class TenantsImpl extends BaseResource implements org.apache.pulsar.client.admin.Tenants, org.apache.pulsar.client.admin.Properties
	public class TenantsImpl : BaseResource, Tenants, Properties
	{
		private readonly WebTarget adminTenants;

		public TenantsImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminTenants = web.path("/admin/v2/tenants");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getTenants() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> Tenants
		{
			get
			{
				try
				{
					return request(adminTenants).get(new GenericTypeAnonymousInnerClass(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly TenantsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(TenantsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.TenantInfo getTenantInfo(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual TenantInfo getTenantInfo(string tenant)
		{
			try
			{
				return request(adminTenants.path(tenant)).get(typeof(TenantInfo));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createTenant(string tenant, TenantInfo config)
		{
			try
			{
				request(adminTenants.path(tenant)).put(Entity.entity(config, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateTenant(string tenant, TenantInfo config)
		{
			try
			{
				request(adminTenants.path(tenant)).post(Entity.entity(config, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteTenant(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteTenant(string tenant)
		{
			try
			{
				request(adminTenants.path(tenant)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		// Compat method names

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createProperty(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createProperty(string tenant, TenantInfo config)
		{
			createTenant(tenant, config);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateProperty(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateProperty(string tenant, TenantInfo config)
		{
			updateTenant(tenant, config);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteProperty(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteProperty(string tenant)
		{
			deleteTenant(tenant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getProperties() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> Properties
		{
			get
			{
				return Tenants;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.TenantInfo getPropertyAdmin(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual TenantInfo getPropertyAdmin(string tenant)
		{
			return getTenantInfo(tenant);
		}

		public virtual WebTarget WebTarget
		{
			get
			{
				return adminTenants;
			}
		}
	}

}