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
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using ResourceQuota = pulsar.common.policies.data.ResourceQuota;

	public interface ResourceQuotas
	{

		/// <summary>
		/// Get default resource quota for new resource bundles.
		/// <para>
		/// Get default resource quota for new resource bundles.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "msgRateIn" : 10,
		///      "msgRateOut" : 30,
		///      "bandwidthIn" : 10000,
		///      "bandwidthOut" : 30000,
		///      "memory" : 100,
		///      "dynamic" : true
		///  }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Permission denied </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.ResourceQuota getDefaultResourceQuota() throws PulsarAdminException;
		ResourceQuota DefaultResourceQuota {get;set;}

		/// <summary>
		/// Set default resource quota for new namespace bundles.
		/// <para>
		/// Set default resource quota for new namespace bundles.
		/// </para>
		/// <para>
		/// The resource quota can be set with these properties:
		/// <ul>
		/// <li><code>msgRateIn</code> : The maximum incoming messages per second.
		/// <li><code>msgRateOut</code> : The maximum outgoing messages per second.
		/// <li><code>bandwidthIn</code> : The maximum inbound bandwidth used.
		/// <li><code>bandwidthOut</code> : The maximum outbound bandwidth used.
		/// <li><code>memory</code> : The maximum memory used.
		/// <li><code>dynamic</code> : allow the quota to be dynamically re-calculated.
		/// </li>
		/// </ul>
		/// 
		/// </para>
		/// <para>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "msgRateIn" : 10,
		///      "msgRateOut" : 30,
		///      "bandwidthIn" : 10000,
		///      "bandwidthOut" : 30000,
		///      "memory" : 100,
		///      "dynamic" : false
		///  }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="quota">
		///             The new ResourceQuota
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setDefaultResourceQuota(org.apache.pulsar.common.policies.data.ResourceQuota quota) throws PulsarAdminException;

		/// <summary>
		/// Get resource quota of a namespace bundle.
		/// <para>
		/// Get resource quota of a namespace bundle.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "msgRateIn" : 10,
		///      "msgRateOut" : 30,
		///      "bandwidthIn" : 10000,
		///      "bandwidthOut" : 30000,
		///      "memory" : 100,
		///      "dynamic" : true
		///  }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Range of bundle {start}_{end}
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Permission denied </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.ResourceQuota getNamespaceBundleResourceQuota(String namespace, String bundle) throws PulsarAdminException;
		ResourceQuota getNamespaceBundleResourceQuota(string @namespace, string bundle);

		/// <summary>
		/// Set resource quota for a namespace bundle.
		/// <para>
		/// Set resource quota for a namespace bundle.
		/// </para>
		/// <para>
		/// The resource quota can be set with these properties:
		/// <ul>
		/// <li><code>msgRateIn</code> : The maximum incoming messages per second.
		/// <li><code>msgRateOut</code> : The maximum outgoing messages per second.
		/// <li><code>bandwidthIn</code> : The maximum inbound bandwidth used.
		/// <li><code>bandwidthOut</code> : The maximum outbound bandwidth used.
		/// <li><code>memory</code> : The maximum memory used.
		/// <li><code>dynamic</code> : allow the quota to be dynamically re-calculated.
		/// </li>
		/// </ul>
		/// 
		/// </para>
		/// <para>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "msgRateIn" : 10,
		///      "msgRateOut" : 30,
		///      "bandwidthIn" : 10000,
		///      "bandwidthOut" : 30000,
		///      "memory" : 100,
		///      "dynamic" : false
		///  }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Bundle range {start}_{end} </param>
		/// <param name="quota">
		///             The new ResourceQuota
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setNamespaceBundleResourceQuota(String namespace, String bundle, org.apache.pulsar.common.policies.data.ResourceQuota quota) throws PulsarAdminException;
		void setNamespaceBundleResourceQuota(string @namespace, string bundle, ResourceQuota quota);

		/// <summary>
		/// Reset resource quota for a namespace bundle to default value.
		/// <para>
		/// Reset resource quota for a namespace bundle to default value.
		/// </para>
		/// <para>
		/// The resource quota policy will fall back to the default.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Bundle range {start}_{end}
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void resetNamespaceBundleResourceQuota(String namespace, String bundle) throws PulsarAdminException;
		void resetNamespaceBundleResourceQuota(string @namespace, string bundle);
	}


}