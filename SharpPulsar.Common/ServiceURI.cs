using System;
using System.Collections.Generic;
using System.Linq;

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
namespace SharpPulsar.Common
{


	/// <summary>
	/// ServiceURI represents service uri within pulsar cluster.
	/// 
	/// <para>This file is based on
	/// <seealso cref="https://github.com/apache/bookkeeper/blob/master/bookkeeper-common/src/main/java/org/apache/bookkeeper/common/net/ServiceURI.java"/>
	/// </para>
	/// </summary>
	public class ServiceUri
	{

		private const string BinaryService = "pulsar";
		private const string HttpService = "http";
		private const string HttpsService = "https";
		private const string SslService = "ssl";

		private const int BinaryPort = 6650;
		private const int BinaryTlsPort = 6651;
		private const int HttpPort = 80;
		private const int HttpsPort = 443;

        private ServiceUri(string serviceName, string[] serviceInfos, string serviceUser, string[] serviceHosts, string servicePath, Uri uri)
        {
            _serviceName = serviceName;
            _serviceInfos = serviceInfos;
            _serviceUser = serviceUser;
            _serviceHosts = serviceHosts;
            _servicePath = servicePath;
            _uri = uri;
        }
		/// <summary>
		/// Create a service uri instance from a uri string.
		/// </summary>
		/// <param name="uriStr"> service uri string </param>
		/// <returns> a service uri instance </returns>
		/// <exception cref="NullPointerException"> if {@code uriStr} is null </exception>
		/// <exception cref="IllegalArgumentException"> if the given string violates RFC&nbsp;2396 </exception>
		public static ServiceUri Create(string uriStr)
		{
			if(string.IsNullOrWhiteSpace(uriStr))
				throw new NullReferenceException("service uri string is null");

			// a service uri first should be a valid java.net.URI
			var uri = new Uri(uriStr);

			return Create(uri);
		}

		/// <summary>
		/// Create a service uri instance from a <seealso cref="URI"/> instance.
		/// </summary>
		/// <param name="uri"> <seealso cref="URI"/> instance </param>
		/// <returns> a service uri instance </returns>
		/// <exception cref="NullPointerException"> if {@code uriStr} is null </exception>
		/// <exception cref="IllegalArgumentException"> if the given string violates RFC&nbsp;2396 </exception>
		public static ServiceUri Create(Uri uri)
		{
			if(uri == null)
				throw new NullReferenceException("service uri instance is null");

			string serviceName;
			string[] serviceInfos;
			var scheme = uri.Scheme;
            {
                scheme = scheme.ToLower();
                const string serviceSep = "+";
                var schemeParts = scheme.Split(serviceSep);
                serviceName = schemeParts[0];
                serviceInfos = new string[schemeParts.Length - 1];
                Array.Copy(schemeParts, 1, serviceInfos, 0, serviceInfos.Length);
            }

            var userAndHostInformation = uri.Authority;
			if(string.IsNullOrWhiteSpace(userAndHostInformation))
				throw new ArgumentNullException("authority component is missing in service uri : " + uri);

			string serviceUser;
			IList<string> serviceHosts;
			var atIndex = userAndHostInformation.IndexOf('@');
			if (atIndex > 0)
			{
				serviceUser = userAndHostInformation.Substring(0, atIndex);
				serviceHosts = userAndHostInformation.Substring(atIndex + 1).Split(new char[] { ',', ';'});
			}
			else
			{
				serviceUser = null;
				serviceHosts = userAndHostInformation.Split(new char[] { ',', ';' });
			}
			serviceHosts = serviceHosts.Select(host => ValidateHostName(serviceName, serviceInfos, host)).ToList();

			var servicePath = uri.AbsolutePath;
			if(string.IsNullOrWhiteSpace(servicePath))
				throw new ArgumentNullException("service path component is missing in service uri : " + uri);

			return new ServiceUri(serviceName, serviceInfos, serviceUser, ((List<string>)serviceHosts).ToArray(), servicePath, uri);
           
        }

		private static string ValidateHostName(string serviceName, string[] serviceInfos, string hostname)
		{
			Uri uri = null;
			try
			{
				uri = new Uri("dummyscheme://" + hostname);
			}
			catch (ArgumentException)
			{
				throw new ArgumentException("Invalid hostname : " + hostname);
			}
			var host = uri.Host;
			if (ReferenceEquals(host, null))
			{
				throw new ArgumentException("Invalid hostname : " + hostname);
			}
			var port = uri.Port;
			if (port == -1)
			{
				port = GetServicePort(serviceName, serviceInfos);
			}
			return host + ":" + port;
		}

		private readonly string _serviceName;
		private readonly string[] _serviceInfos;
		private readonly string _serviceUser;
		private readonly string[] _serviceHosts;
		private readonly string _servicePath;
		private readonly Uri _uri;

        public virtual string ServiceUser => _serviceUser;
        public virtual string ServicePath => _servicePath;
        public virtual Uri Uri => _uri;
		public virtual string[] ServiceInfos => _serviceInfos;

		public virtual string[] ServiceHosts => _serviceHosts;

        public virtual string ServiceScheme
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_serviceName))
				{
					return null;
				}
				else
				{
					if (_serviceInfos.Length == 0)
					{
						return _serviceName;
					}
					else
					{
						return _serviceName + "+" + string.Join('+', _serviceInfos);
					}
				}
			}
		}

		private static int GetServicePort(string serviceName, string[] serviceInfos)
		{
			int port;
			switch (serviceName.ToLower())
			{
				case BinaryService:
					if (serviceInfos.Length == 0)
					{
						port = BinaryPort;
					}
					else if (serviceInfos.Length == 1 && serviceInfos[0].ToLower().Equals(SslService))
					{
						port = BinaryTlsPort;
					}
					else
					{
						throw new ArgumentException("Invalid pulsar service : " + serviceName + "+" + serviceInfos);
					}
					break;
				case HttpService:
					port = HttpPort;
					break;
				case HttpsService:
					port = HttpsPort;
					break;
				default:
					throw new ArgumentException("Invalid pulsar service : " + serviceName);
			}
			return port;
		}

	}

}