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
	/// Admin interface for ResourceGroups management.
	/// </summary>

	public interface IResourceGroups
	{

			/// <summary>
			/// Get the list of resourcegroups.
			/// <p/>
			/// Get the list of all the resourcegroup.
			/// <p/>
			/// Response Example:
			/// 
			/// <pre>
			/// <code>["resourcegroup1",
			///  "resourcegroup2",
			///  "resourcegroup3"]</code>
			/// </pre>
			/// </summary>
			/// <exception cref="PulsarAdminException.NotAuthorizedException"> Don't have admin permission </exception>
			/// <exception cref="PulsarAdminException">                        Unexpected error </exception>
			IList<string> ResourceGroups {get;}

			/// <summary>
			/// Get the list of resourcegroups asynchronously.
			/// <p/>
			/// Get the list of all the resourcegrops.
			/// <p/>
			/// Response Example:
			/// 
			/// <pre>
			/// <code>["resourcegroup1",
			///  "resourcegroup2",
			///  "resourcegroup3"]</code>
			/// </pre>
			/// </summary>
			ValueTask<IList<string>> ResourceGroupsAsync {get;}


			/// <summary>
			/// Get configuration for a resourcegroup.
			/// <p/>
			/// Get configuration specified for a resourcegroup.
			/// <p/>
			/// Response Example:
			/// 
			/// <pre>
			/// <code>
			///     "publishRateInMsgs" : "value",
			///     "PublishRateInBytes" : "value",
			///     "DispatchRateInMsgs" : "value",
			///     "DispatchRateInBytes" : "value"
			/// </code>
			/// </pre>
			/// </summary>
			/// <param name="resourcegroup"> String resourcegroup </param>
			/// <exception cref="PulsarAdminException.NotAuthorizedException"> You don't have admin permission </exception>
			/// <exception cref="PulsarAdminException.NotFoundException">      Resourcegroup does not exist </exception>
			/// <exception cref="PulsarAdminException">                        Unexpected error </exception>
			/// <seealso cref="ResourceGroup"
			/// <para>
			/// */>
			ResourceGroup GetResourceGroup(string resourcegroup);

			/// <summary>
			/// Get policies for a namespace asynchronously.
			/// <p/>
			/// Get cnfiguration specified for a resourcegroup.
			/// <p/>
			/// Response example:
			/// 
			/// <pre>
			/// <code>
			///     "publishRateInMsgs" : "value",
			///     "PublishRateInBytes" : "value",
			///     "DispatchRateInMsgs" : "value",
			///     "DspatchRateInBytes" : "value"
			/// </code>
			/// </pre>
			/// </summary>
			/// <param name="resourcegroup"> Namespace name </param>
			/// <seealso cref="ResourceGroup"/>
			ValueTask<ResourceGroup> GetResourceGroupAsync(string resourcegroup);

			/// <summary>
			/// Create a new resourcegroup.
			/// <p/>
			/// Creates a new reourcegroup with the configuration specified.
			/// </summary>
			/// <param name="name">       resourcegroup name </param>
			/// <param name="resourcegroup"> ResourceGroup configuration </param>
			/// <exception cref="PulsarAdminException.NotAuthorizedException"> You don't have admin permission </exception>
			/// <exception cref="PulsarAdminException.ConflictException">      Resourcegroup already exists </exception>
			/// <exception cref="PulsarAdminException">                        Unexpected error </exception>
			void CreateResourceGroup(string name, ResourceGroup resourcegroup);

			/// <summary>
			/// Create a new resourcegroup.
			/// <p/>
			/// Creates a new resourcegroup with the configuration specified.
			/// </summary>
			/// <param name="name">           resourcegroup name </param>
			/// <param name="resourcegroup"> ResourceGroup configuration. </param>
			ValueTask CreateResourceGroupAsync(string name, ResourceGroup resourcegroup);

			/// <summary>
			/// Update the configuration for a ResourceGroup.
			/// <p/>
			/// This operation requires Pulsar super-user privileges.
			/// </summary>
			/// <param name="name">          resourcegroup name </param>
			/// <param name="resourcegroup"> resourcegroup configuration
			/// </param>
			/// <exception cref="PulsarAdminException.NotAuthorizedException">
			///             Don't have admin permission </exception>
			/// <exception cref="PulsarAdminException.NotFoundException">
			///             ResourceGroup does not exist </exception>
			/// <exception cref="PulsarAdminException">
			///             Unexpected error </exception>
			void UpdateResourceGroup(string name, ResourceGroup resourcegroup);

			/// <summary>
			/// Update the configuration for a ResourceGroup.
			/// <p/>
			/// This operation requires Pulsar super-user privileges.
			/// </summary>
			/// <param name="name">          resourcegroup name </param>
			/// <param name="resourcegroup"> resourcegroup configuration </param>
			ValueTask UpdateResourceGroupAsync(string name, ResourceGroup resourcegroup);


			/// <summary>
			/// Delete an existing resourcegroup.
			/// <p/>
			/// The resourcegroup needs to unused and not attached to any entity.
			/// </summary>
			/// <param name="resourcegroup"> Resourcegroup name </param>
			/// <exception cref="PulsarAdminException.NotAuthorizedException"> You don't have admin permission </exception>
			/// <exception cref="PulsarAdminException.NotFoundException">      Resourcegroup does not exist </exception>
			/// <exception cref="PulsarAdminException.ConflictException">      Resourcegroup is in use </exception>
			/// <exception cref="PulsarAdminException">                        Unexpected error </exception>
			void DeleteResourceGroup(string resourcegroup);

			/// <summary>
			/// Delete an existing resourcegroup.
			/// <p/>
			/// The resourcegroup needs to unused and not attached to any entity.
			/// </summary>
			/// <param name="resourcegroup"> Resourcegroup name </param>

			ValueTask DeleteResourceGroupAsync(string resourcegroup);

	}
}