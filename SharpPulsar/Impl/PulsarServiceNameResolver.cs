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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;

	using PlatformDependent = io.netty.util.@internal.PlatformDependent;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using InvalidServiceURL = org.apache.pulsar.client.api.PulsarClientException.InvalidServiceURL;
	using ServiceURI = org.apache.pulsar.common.net.ServiceURI;

	/// <summary>
	/// The default implementation of <seealso cref="ServiceNameResolver"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class PulsarServiceNameResolver implements ServiceNameResolver
	public class PulsarServiceNameResolver : ServiceNameResolver
	{

		private volatile ServiceURI serviceUri;
		private volatile string serviceUrl;
		private volatile int currentIndex;
		private volatile IList<InetSocketAddress> addressList;

		public virtual InetSocketAddress resolveHost()
		{
			IList<InetSocketAddress> list = addressList;
			checkState(list != null, "No service url is provided yet");
			checkState(list.Count > 0, "No hosts found for service url : " + serviceUrl);
			if (list.Count == 1)
			{
				return list[0];
			}
			else
			{
				currentIndex = (currentIndex + 1) % list.Count;
				return list[currentIndex];

			}
		}

		public virtual URI resolveHostUri()
		{
			InetSocketAddress host = resolveHost();
			string hostUrl = serviceUri.ServiceScheme + "://" + host.HostName + ":" + host.Port;
			return URI.create(hostUrl);
		}

		public virtual string ServiceUrl
		{
			get
			{
				return serviceUrl;
			}
		}

		public virtual ServiceURI ServiceUri
		{
			get
			{
				return serviceUri;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateServiceUrl(String serviceUrl) throws org.apache.pulsar.client.api.PulsarClientException.InvalidServiceURL
		public virtual void updateServiceUrl(string serviceUrl)
		{
			ServiceURI uri;
			try
			{
				uri = ServiceURI.create(serviceUrl);
			}
			catch (System.ArgumentException iae)
			{
				log.error("Invalid service-url {} provided {}", serviceUrl, iae.Message, iae);
				throw new InvalidServiceURL(iae);
			}

			string[] hosts = uri.ServiceHosts;
			IList<InetSocketAddress> addresses = new List<InetSocketAddress>(hosts.Length);
			foreach (string host in hosts)
			{
				string hostUrl = uri.ServiceScheme + "://" + host;
				try
				{
					URI hostUri = new URI(hostUrl);
					addresses.Add(InetSocketAddress.createUnresolved(hostUri.Host, hostUri.Port));
				}
				catch (URISyntaxException e)
				{
					log.error("Invalid host provided {}", hostUrl, e);
					throw new InvalidServiceURL(e);
				}
			}
			this.addressList = addresses;
			this.serviceUrl = serviceUrl;
			this.serviceUri = uri;
			this.currentIndex = randomIndex(addresses.Count);
		}

		private static int randomIndex(int numAddresses)
		{
			return numAddresses == 1 ? 0 : PlatformDependent.threadLocalRandom().Next(numAddresses);
		}
	}

}