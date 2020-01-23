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
namespace SharpPulsar.Impl
{
	using InvalidServiceURL = SharpPulsar.Api.PulsarClientException.InvalidServiceURL;
	using ServiceURI = Org.Apache.Pulsar.Common.Net.ServiceURI;

	/// <summary>
	/// A service name resolver to resolve real socket address.
	/// </summary>
	public interface ServiceNameResolver
	{

		/// <summary>
		/// Resolve pulsar service url.
		/// </summary>
		/// <returns> resolve the service url to return a socket address </returns>
		InetSocketAddress ResolveHost();

		/// <summary>
		/// Resolve pulsar service url
		/// @return
		/// </summary>
		URI ResolveHostUri();

		/// <summary>
		/// Get service url.
		/// </summary>
		/// <returns> service url </returns>
		string ServiceUrl {get;}

		/// <summary>
		/// Get service uri.
		/// </summary>
		/// <returns> service uri </returns>
		ServiceURI ServiceUri {get;}

		/// <summary>
		/// Update service url.
		/// </summary>
		/// <param name="serviceUrl"> service url </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateServiceUrl(String serviceUrl) throws SharpPulsar.api.PulsarClientException.InvalidServiceURL;
		void UpdateServiceUrl(string ServiceUrl);

	}

}