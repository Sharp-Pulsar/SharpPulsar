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
	/// Admin interface on interacting with resource quotas.
	/// </summary>
	public interface IResourceQuotas
	{

		/// <summary>
		/// Get default resource quota for new resource bundles.
		/// <p/>
		/// Get default resource quota for new resource bundles.
		/// <p/>
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
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Permission denied </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ResourceQuota DefaultResourceQuota {get;set;}

		/// <summary>
		/// Get default resource quota for new resource bundles asynchronously.
		/// <p/>
		/// Get default resource quota for new resource bundles.
		/// <p/>
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
		/// </summary>
		ValueTask<ResourceQuota> DefaultResourceQuotaAsync {get;}

		/// <summary>
		/// Set default resource quota for new namespace bundles.
		/// <p/>
		/// Set default resource quota for new namespace bundles.
		/// <p/>
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
		/// <p/>
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
		/// </summary>
		/// <param name="quota">
		///             The new ResourceQuota
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

       void SetDefaultResourceQuota(ResourceQuota quota);

		/// <summary>
		/// Set default resource quota for new namespace bundles asynchronously.
		/// <p/>
		/// Set default resource quota for new namespace bundles.
		/// <p/>
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
		/// <p/>
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
		/// </summary>
		/// <param name="quota">
		///             The new ResourceQuota </param>
		ValueTask SetDefaultResourceQuotaAsync(ResourceQuota quota);

		/// <summary>
		/// Get resource quota of a namespace bundle.
		/// <p/>
		/// Get resource quota of a namespace bundle.
		/// <p/>
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

		ResourceQuota GetNamespaceBundleResourceQuota(string @namespace, string bundle);

		/// <summary>
		/// Get resource quota of a namespace bundle asynchronously.
		/// <p/>
		/// Get resource quota of a namespace bundle.
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Range of bundle {start}_{end}
		///  </param>
		ValueTask<ResourceQuota> GetNamespaceBundleResourceQuotaAsync(string @namespace, string bundle);

		/// <summary>
		/// Set resource quota for a namespace bundle.
		/// <p/>
		/// Set resource quota for a namespace bundle.
		/// <p/>
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
		/// <p/>
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

		void SetNamespaceBundleResourceQuota(string @namespace, string bundle, ResourceQuota quota);

		/// <summary>
		/// Set resource quota for a namespace bundle asynchronously.
		/// <p/>
		/// Set resource quota for a namespace bundle.
		/// <p/>
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
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Bundle range {start}_{end} </param>
		/// <param name="quota">
		///             The new ResourceQuota
		///  </param>
		ValueTask SetNamespaceBundleResourceQuotaAsync(string @namespace, string bundle, ResourceQuota quota);

		/// <summary>
		/// Reset resource quota for a namespace bundle to default value.
		/// <p/>
		/// Reset resource quota for a namespace bundle to default value.
		/// <p/>
		/// The resource quota policy will fall back to the default.
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

		void ResetNamespaceBundleResourceQuota(string @namespace, string bundle);

		/// <summary>
		/// Reset resource quota for a namespace bundle to default value asynchronously.
		/// <p/>
		/// Reset resource quota for a namespace bundle to default value.
		/// <p/>
		/// The resource quota policy will fall back to the default.
		/// </summary>
		/// <param name="namespace">
		///             Namespace name </param>
		/// <param name="bundle">
		///             Bundle range {start}_{end}
		///  </param>
		ValueTask ResetNamespaceBundleResourceQuotaAsync(string @namespace, string bundle);
	}


}