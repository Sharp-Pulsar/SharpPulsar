using SharpPulsar.Common;
using System;
using System.Collections.Generic;
using System.Net;
using DotNetty.Common.Internal;
using Microsoft.Extensions.Logging;

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
    /// <summary>
	/// The default implementation of <seealso cref="ServiceNameResolver"/>.
	/// </summary>
	public class PulsarServiceNameResolver : ServiceNameResolver
    {
		private  int _currentIndex;
		private volatile IList<IPEndPoint> _addressList;
        private ServiceUri _serviceUri;

        public IList<IPEndPoint> AddressList()
        {
            return _addressList;
        }

		public  IPEndPoint ResolveHost()
		{
			var list = _addressList;
			if(list == null)
				throw new ArgumentException("No service url is provided yet");
			if(list.Count < 1)
				throw new ArgumentException("No hosts found for service url : " + ServiceUrl);
			if (list.Count == 1)
			{
				return list[0];
			}

            _currentIndex = (_currentIndex + 1) % list.Count;
            return list[_currentIndex];
        }

		public Uri ResolveHostUri()
		{
			var host = ResolveHost();
			var hostUrl = ServiceUri.ServiceScheme + "://" + Dns.GetHostEntry(host.Address).HostName + ":" + host.Port;
			return new Uri(hostUrl);
		}

        public string ServiceUrl { get; }

		public ServiceUri ServiceUri
        {
            get => _serviceUri;
            set => _serviceUri = value;
        }

        public void UpdateServiceUrl(string serviceUrl)
		{
			ServiceUri uri;
			try
			{
				uri = ServiceUri.Create(serviceUrl);
			}
			catch (System.Exception iae)
			{
				Log.LogWarning("Invalid service-url {} provided {}", serviceUrl, iae.Message, iae);
				throw;
			}

			var hosts = uri.ServiceHosts;
			IList<IPEndPoint> addresses = new List<IPEndPoint>(hosts.Length);
			foreach (var host in hosts)
			{
				var hostUrl = uri.ServiceScheme + "://" + host;
				try
				{
					var hostUri = new Uri(hostUrl);
					addresses.Add(new IPEndPoint(Dns.GetHostAddresses(hostUri.Host)[0], hostUri.Port));
				}
				catch (UriFormatException e)
				{
					Log.LogError("Invalid host provided {}", hostUrl);
					throw;
				}
			}
			_addressList = addresses;
			_serviceUri = uri;
			_currentIndex = RandomIndex(addresses.Count);
		}

		private static int RandomIndex(int numAddresses)
		{
			return numAddresses == 1 ? 0 : PlatformDependent.GetThreadLocalRandom().Next(numAddresses);
		}
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(PulsarServiceNameResolver));
	}

}