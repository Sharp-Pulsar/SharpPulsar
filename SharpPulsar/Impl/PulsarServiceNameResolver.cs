using SharpPulsar.Common;
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
namespace SharpPulsar.Impl
{
    using static SharpPulsar.Exception.PulsarClientException;
    using PlatformDependent = io.netty.util.@internal.PlatformDependent;

	/// <summary>
	/// The default implementation of <seealso cref="ServiceNameResolver"/>.
	/// </summary>
	public class PulsarServiceNameResolver : ServiceNameResolver
	{

		public ServiceURI ServiceUrl;
		private  int currentIndex;
		private volatile IList<Uri> addressList;

		public  Uri ResolveHost()
		{
			var List = addressList;
			if(List == null)
				throw new ArgumentNullException("No service url is provided yet");
			if(List.Count < 1)
				throw new ArgumentNullException("No hosts found for service url : " + ServiceUrl);
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

		public Uri ResolveHostUri()
		{
			var Host = ResolveHost();
			var HostUrl = ServiceUrl.ServiceScheme + "://" + Host.Host + ":" + Host.Port;
			return new Uri(HostUrl);
		}


		public void UpdateServiceUrl(string ServiceUrl)
		{
			ServiceURI Uri;
			try
			{
				Uri = ServiceURI.Create(ServiceUrl);
			}
			catch (ArgumentException Iae)
			{
				log.error("Invalid service-url {} provided {}", ServiceUrl, Iae.Message, Iae);
				throw new InvalidServiceURL(Iae.Message);
			}

			var Hosts = Uri.ServiceHosts;
			IList<Uri> Addresses = new List<Uri>(Hosts.Length);
			foreach (var Host in Hosts)
			{
				var HostUrl = Uri.ServiceScheme + "://" + Host;
				try
				{
					Uri HostUri = new uri(HostUrl);
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