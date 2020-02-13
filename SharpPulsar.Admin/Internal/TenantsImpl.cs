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
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using TenantInfo = Org.Apache.Pulsar.Common.Policies.Data.TenantInfo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public class TenantsImpl extends BaseResource implements org.apache.pulsar.client.admin.Tenants, org.apache.pulsar.client.admin.Properties
	public class TenantsImpl : BaseResource, Tenants, Properties
	{
		public virtual WebTarget {get;}

		public TenantsImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			WebTarget = Web.path("/admin/v2/tenants");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getTenants() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> Tenants
		{
			get
			{
				try
				{
					return Request(WebTarget).get(new GenericTypeAnonymousInnerClass(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly TenantsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(TenantsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.TenantInfo getTenantInfo(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override TenantInfo GetTenantInfo(string Tenant)
		{
			try
			{
				return Request(WebTarget.path(Tenant)).get(typeof(TenantInfo));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateTenant(string Tenant, TenantInfo Config)
		{
			try
			{
				Request(WebTarget.path(Tenant)).put(Entity.entity(Config, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateTenant(string Tenant, TenantInfo Config)
		{
			try
			{
				Request(WebTarget.path(Tenant)).post(Entity.entity(Config, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteTenant(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteTenant(string Tenant)
		{
			try
			{
				Request(WebTarget.path(Tenant)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		// Compat method names

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createProperty(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateProperty(string Tenant, TenantInfo Config)
		{
			CreateTenant(Tenant, Config);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateProperty(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateProperty(string Tenant, TenantInfo Config)
		{
			UpdateTenant(Tenant, Config);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteProperty(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteProperty(string Tenant)
		{
			DeleteTenant(Tenant);
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
		public override TenantInfo GetPropertyAdmin(string Tenant)
		{
			return GetTenantInfo(Tenant);
		}

	}

}