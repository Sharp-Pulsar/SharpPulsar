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
	/// Admin interface for tenants management.
	/// </summary>
	public interface ITenants
	{
		/// <summary>
		/// Get the list of tenants.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant", "other-tenant", "third-tenant"]</code>
		/// </pre>
		/// </summary>
		/// <returns> the list of Pulsar tenants </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> Tenants {get;}

		/// <summary>
		/// Get the list of tenants asynchronously.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant", "other-tenant", "third-tenant"]</code>
		/// </pre>
		/// </summary>
		/// <returns> the list of Pulsar tenants </returns>
		ValueTask<IList<string>> TenantsAsync {get;}

		/// <summary>
		/// Get the config of the tenant.
		/// <p/>
		/// Get the admin configuration for a given tenant.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <returns> the tenant configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
		TenantInfo GetTenantInfo(string tenant);

		/// <summary>
		/// Get the config of the tenant asynchronously.
		/// <p/>
		/// Get the admin configuration for a given tenant.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <returns> the tenant configuration </returns>
		ValueTask<TenantInfo> GetTenantInfoAsync(string tenant);

		/// <summary>
		/// Create a new tenant.
		/// <p/>
		/// Provisions a new tenant. This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="config">
		///            Config data
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="ConflictException">
		///             Tenant already exists </exception>
		/// <exception cref="PreconditionFailedException">
		///             Tenant name is not valid </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void CreateTenant(string tenant, TenantInfo config);

		/// <summary>
		/// Create a new tenant asynchronously.
		/// <p/>
		/// Provisions a new tenant. This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="config">
		///            Config data </param>
		ValueTask CreateTenantAsync(string tenant, TenantInfo config);

		/// <summary>
		/// Update the admins for a tenant.
		/// <p/>
		/// This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="config">
		///            Config data
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateTenant(string tenant, TenantInfo config);

		/// <summary>
		/// Update the admins for a tenant asynchronously.
		/// <p/>
		/// This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="config">
		///            Config data </param>
		ValueTask UpdateTenantAsync(string tenant, TenantInfo config);

		/// <summary>
		/// Delete an existing tenant.
		/// <p/>
		/// Delete a tenant and all namespaces and topics under it.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             The tenant does not exist </exception>
		/// <exception cref="ConflictException">
		///             The tenant still has active namespaces </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteTenant(string tenant);

		/// <summary>
		/// Delete an existing tenant.
		/// <p/>
		/// Force flag delete a tenant forcefully and all namespaces and topics under it.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="force">
		///            Delete tenant forcefully
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             The tenant does not exist </exception>
		/// <exception cref="ConflictException">
		///             The tenant still has active namespaces </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteTenant(string tenant, bool force);

		/// <summary>
		/// Delete an existing tenant asynchronously.
		/// <p/>
		/// Delete a tenant and all namespaces and topics under it.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		ValueTask DeleteTenantAsync(string tenant);

		/// <summary>
		/// Delete an existing tenant asynchronously.
		/// <p/>
		/// Force flag delete a tenant forcefully and all namespaces and topics under it.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="force">
		///            Delete tenant forcefully </param>
		ValueTask DeleteTenantAsync(string tenant, bool force);
	}

}