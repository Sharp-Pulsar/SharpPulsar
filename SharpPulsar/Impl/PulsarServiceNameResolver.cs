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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;

	using PlatformDependent = io.netty.util.@internal.PlatformDependent;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using InvalidServiceURL = SharpPulsar.Api.PulsarClientException.InvalidServiceURL;
	using ServiceURI = Org.Apache.Pulsar.Common.Net.ServiceURI;

	/// <summary>
	/// The default implementation of <seealso cref="ServiceNameResolver"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class PulsarServiceNameResolver implements ServiceNameResolver
	public class PulsarServiceNameResolver : ServiceNameResolver
	{

		public virtual ServiceUri {get;}
		public virtual ServiceUrl {get;}
		private volatile int currentIndex;
		private volatile IList<InetSocketAddress> addressList;

		public override InetSocketAddress ResolveHost()
		{
			IList<InetSocketAddress> List = addressList;
			checkState(List != null, "No service url is provided yet");
			checkState(List.Count > 0, "No hosts found for service url : " + ServiceUrl);
			if (List.Count == 1)
			{
				return List[0];
			}
			else
			{
				currentIndex = (currentIndex + 1) % List.Count;
				return List[currentIndex];

			}
		}

		public override URI ResolveHostUri()
		{
			InetSocketAddress Host = ResolveHost();
			string HostUrl = ServiceUri.ServiceScheme + "://" + Host.HostName + ":" + Host.Port;
			return URI.create(HostUrl);
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateServiceUrl(String serviceUrl) throws SharpPulsar.api.PulsarClientException.InvalidServiceURL
		public override void UpdateServiceUrl(string ServiceUrl)
		{
			ServiceURI Uri;
			try
			{
				Uri = ServiceURI.create(ServiceUrl);
			}
			catch (System.ArgumentException Iae)
			{
				log.error("Invalid service-url {} provided {}", ServiceUrl, Iae.Message, Iae);
				throw new InvalidServiceURL(Iae);
			}

			string[] Hosts = Uri.ServiceHosts;
			IList<InetSocketAddress> Addresses = new List<InetSocketAddress>(Hosts.Length);
			foreach (string Host in Hosts)
			{
				string HostUrl = Uri.ServiceScheme + "://" + Host;
				try
				{
					URI HostUri = new URI(HostUrl);
					Addresses.Add(InetSocketAddress.createUnresolved(HostUri.Host, HostUri.Port));
				}
				catch (URISyntaxException E)
				{
					log.error("Invalid host provided {}", HostUrl, E);
					throw new InvalidServiceURL(E);
				}
			}
			this.addressList = Addresses;
			this.ServiceUrl = ServiceUrl;
			this.ServiceUri = Uri;
			this.currentIndex = RandomIndex(Addresses.Count);
		}

		private static int RandomIndex(int NumAddresses)
		{
			return NumAddresses == 1 ? 0 : PlatformDependent.threadLocalRandom().Next(NumAddresses);
		}
	}

}