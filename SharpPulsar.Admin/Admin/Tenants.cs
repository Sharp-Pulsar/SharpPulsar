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

	using ConflictException = PulsarAdminException.ConflictException;
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using TenantInfo = pulsar.common.policies.data.TenantInfo;

	/// <summary>
	/// Admin interface for tenants management
	/// </summary>
	public interface Tenants
	{
		/// <summary>
		/// Get the list of tenants.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant", "other-tenant", "third-tenant"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> the list of Pulsar tenants </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getTenants() throws PulsarAdminException;
		IList<string> getTenants();

		/// <summary>
		/// Get the config of the tenant.
		/// <para>
		/// Get the admin configuration for a given tenant.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.TenantInfo getTenantInfo(String tenant) throws PulsarAdminException;
		TenantInfo getTenantInfo(string tenant);

		/// <summary>
		/// Create a new tenant.
		/// <para>
		/// Provisions a new tenant. This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws PulsarAdminException;
		void createTenant(string tenant, TenantInfo config);

		/// <summary>
		/// Update the admins for a tenant.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateTenant(String tenant, org.apache.pulsar.common.policies.data.TenantInfo config) throws PulsarAdminException;
		void updateTenant(string tenant, TenantInfo config);

		/// <summary>
		/// Delete an existing tenant.
		/// <para>
		/// Delete a tenant and all namespaces and topics under it.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteTenant(String tenant) throws PulsarAdminException;
		void deleteTenant(string tenant);
	}

}