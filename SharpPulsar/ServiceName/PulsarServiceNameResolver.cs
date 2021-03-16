using SharpPulsar.Common;
using System;
using System.Collections.Generic;
using DotNetty.Common.Internal;
using Akka.Event;
using System.Net;

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
namespace SharpPulsar.ServiceName
{
    /// <summary>
	/// The default implementation of <seealso cref="ServiceNameResolver"/>.
	/// </summary>
	public class PulsarServiceNameResolver : ServiceNameResolver
    {
        private int _currentIndex;
        private IList<Uri> _addressList;
        private ServiceUri _serviceUri;
        private ILoggingAdapter _log;

        public PulsarServiceNameResolver(ILoggingAdapter log)
        {
            _log = log;
        }

        public IList<Uri> AddressList()
        {
            return _addressList;
        }

        public Uri ResolveHost()
        {
            var list = _addressList;
            if (list == null)
                throw new ArgumentException("No service url is provided yet");
            if (list.Count < 1)
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
            var hostUrl = ServiceUri.ServiceScheme + "://" + host.Host + ":" + host.Port;
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
            catch (Exception iae)
            {
                _log.Warning($"Invalid service-url {serviceUrl} provided {iae}");
                throw;
            }

            var hosts = uri.ServiceHosts;
            IList<Uri> addresses = new List<Uri>(hosts.Length);
            foreach (var host in hosts)
            {
                var hostUrl = uri.ServiceScheme + "://" + host;
                try
                {
                    var hostUri = new Uri(hostUrl);
                    addresses.Add(hostUri);
                }
                catch (UriFormatException e)
                {
                    _log.Error($"Invalid host provided {hostUrl}");
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
    }
    public static class UrlEx
    {
        public static DnsEndPoint ToDnsEndPoint(this Uri uri)
        {
            return new DnsEndPoint(uri.Host, uri.Port);
        }
    }
}