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
namespace org.apache.pulsar.client.admin
{

	using ConflictException = org.apache.pulsar.client.admin.PulsarAdminException.ConflictException;
	using NotAuthorizedException = org.apache.pulsar.client.admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = org.apache.pulsar.client.admin.PulsarAdminException.NotFoundException;
	using PreconditionFailedException = org.apache.pulsar.client.admin.PulsarAdminException.PreconditionFailedException;
	using TenantInfo = org.apache.pulsar.common.policies.data.TenantInfo;

	/// <summary>
	/// Admin interface for properties management
	/// </summary>
	/// @deprecated see <seealso cref="Tenants"/> from <seealso cref="PulsarAdmin.tenants()"/> 
	[Obsolete("see <seealso cref=\"Tenants\"/> from <seealso cref=\"PulsarAdmin.tenants()\"/>")]
	public interface Properties
	{
		/// <summary>
		/// Get the list of properties.
		/// <para>
		/// Get the list of all the properties.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-property", "other-property", "third-property"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> the list of Pulsar tenants properties </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getProperties() throws PulsarAdminException;
		IList<string> getProperties();

		/// <summary>
		/// Get the config of the property.
		/// <para>
		/// Get the admin configuration for a given property.
		/// 
		/// </para>
		/// </summary>
		/// <param name="property">
		///            Property name </param>
		/// <returns> the property configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Property does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.TenantInfo getPropertyAdmin(String property) throws PulsarAdminException;
		TenantInfo getPropertyAdmin(string property);

		/// <summary>
		/// Create a new property.
		/// <para>
		/// Provisions a new property. This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
		/// </summary>
		/// <param name="property">
		///            Property name </param>
		/// <param name="config">
		///            Config data
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="ConflictException">
		///             Property already exists </exception>
		/// <exception cref="PreconditionFailedException">
		///             Property name is not valid </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createProperty(String property, org.apache.pulsar.common.policies.data.TenantInfo config) throws PulsarAdminException;
		void createProperty(string property, TenantInfo config);

		/// <summary>
		/// Update the admins for a property.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
		/// </summary>
		/// <param name="property">
		///            Property name </param>
		/// <param name="config">
		///            Config data
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Property does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateProperty(String property, org.apache.pulsar.common.policies.data.TenantInfo config) throws PulsarAdminException;
		void updateProperty(string property, TenantInfo config);

		/// <summary>
		/// Delete an existing property.
		/// <para>
		/// Delete a property and all namespaces and topics under it.
		/// 
		/// </para>
		/// </summary>
		/// <param name="property">
		///            Property name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             The property does not exist </exception>
		/// <exception cref="ConflictException">
		///             The property still has active namespaces </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteProperty(String property) throws PulsarAdminException;
		void deleteProperty(string property);
	}

}