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
	public class ServiceURI
	{

		private const string BinaryService = "pulsar";
		private const string HttpService = "http";
		private const string HttpsService = "https";
		private const string SslService = "ssl";

		private const int BinaryPort = 6650;
		private const int BinaryTlsPort = 6651;
		private const int HttpPort = 80;
		private const int HttpsPort = 443;

		/// <summary>
		/// Create a service uri instance from a uri string.
		/// </summary>
		/// <param name="uriStr"> service uri string </param>
		/// <returns> a service uri instance </returns>
		/// <exception cref="NullPointerException"> if {@code uriStr} is null </exception>
		/// <exception cref="IllegalArgumentException"> if the given string violates RFC&nbsp;2396 </exception>
		public static ServiceURI Create(string UriStr)
		{
			if(string.IsNullOrWhiteSpace(UriStr))
				throw new NullReferenceException("service uri string is null");

			// a service uri first should be a valid java.net.URI
			Uri Uri = new Uri(UriStr);

			return Create(Uri);
		}

		/// <summary>
		/// Create a service uri instance from a <seealso cref="URI"/> instance.
		/// </summary>
		/// <param name="uri"> <seealso cref="URI"/> instance </param>
		/// <returns> a service uri instance </returns>
		/// <exception cref="NullPointerException"> if {@code uriStr} is null </exception>
		/// <exception cref="IllegalArgumentException"> if the given string violates RFC&nbsp;2396 </exception>
		public static ServiceURI Create(Uri Uri)
		{
			if(Uri == null)
				throw new NullReferenceException("service uri instance is null");

			string ServiceName;
			string[] ServiceInfos;
			string Scheme = Uri.Scheme;
			if (null != Scheme)
			{
				Scheme = Scheme.ToLower();
				const string ServiceSep = "+";
				string[] SchemeParts = Scheme.Split(ServiceSep);
				ServiceName = SchemeParts[0];
				ServiceInfos = new string[SchemeParts.Length - 1];
				Array.Copy(SchemeParts, 1, ServiceInfos, 0, ServiceInfos.Length);
			}
			else
			{
				ServiceName = null;
				ServiceInfos = new string[0];
			}

			string UserAndHostInformation = Uri.Authority;
			if(string.IsNullOrWhiteSpace(UserAndHostInformation))
				throw new ArgumentNullException("authority component is missing in service uri : " + Uri);

			string ServiceUser;
			IList<string> ServiceHosts;
			int AtIndex = UserAndHostInformation.IndexOf('@');
			if (AtIndex > 0)
			{
				ServiceUser = UserAndHostInformation.Substring(0, AtIndex);
				ServiceHosts = UserAndHostInformation.Substring(AtIndex + 1).Split(new char[] { ',', ';'});
			}
			else
			{
				ServiceUser = null;
				ServiceHosts = UserAndHostInformation.Split(new char[] { ',', ';' });
			}
			ServiceHosts = ServiceHosts.Select(host => ValidateHostName(ServiceName, ServiceInfos, host)).ToList();

			string ServicePath = Uri.AbsolutePath;
			if(string.IsNullOrWhiteSpace(ServicePath))
				throw new ArgumentNullException("service path component is missing in service uri : " + Uri);

			return new ServiceURI(ServiceName, ServiceInfos, ServiceUser, ((List<string>)ServiceHosts).ToArray(), ServicePath, Uri);
		}

		private static string ValidateHostName(string ServiceName, string[] ServiceInfos, string Hostname)
		{
			Uri Uri = null;
			try
			{
				Uri = new Uri("dummyscheme://" + Hostname);
			}
			catch (System.ArgumentException)
			{
				throw new System.ArgumentException("Invalid hostname : " + Hostname);
			}
			string Host = Uri.Host;
			if (string.ReferenceEquals(Host, null))
			{
				throw new System.ArgumentException("Invalid hostname : " + Hostname);
			}
			int Port = Uri.Port;
			if (Port == -1)
			{
				Port = GetServicePort(ServiceName, ServiceInfos);
			}
			return Host + ":" + Port;
		}

		private readonly string serviceName;
		private readonly string[] serviceInfos;
		private readonly string serviceUser;
		private readonly string[] serviceHosts;
		private readonly string servicePath;
		private readonly Uri uri;

		public virtual string[] ServiceInfos
		{
			get
			{
				return serviceInfos;
			}
		}

		public virtual string[] ServiceHosts
		{
			get
			{
				return serviceHosts;
			}
		}

		public virtual string ServiceScheme
		{
			get
			{
				if (null == serviceName)
				{
					return null;
				}
				else
				{
					if (serviceInfos.Length == 0)
					{
						return serviceName;
					}
					else
					{
						return serviceName + "+" + string.Join('+', serviceInfos);
					}
				}
			}
		}

		private static int GetServicePort(string ServiceName, string[] ServiceInfos)
		{
			int Port;
			switch (ServiceName.ToLower())
			{
				case BinaryService:
					if (ServiceInfos.Length == 0)
					{
						Port = BinaryPort;
					}
					else if (ServiceInfos.Length == 1 && ServiceInfos[0].ToLower().Equals(SslService))
					{
						Port = BinaryTlsPort;
					}
					else
					{
						throw new System.ArgumentException("Invalid pulsar service : " + ServiceName + "+" + ServiceInfos);
					}
					break;
				case HttpService:
					Port = HttpPort;
					break;
				case HttpsService:
					Port = HttpsPort;
					break;
				default:
					throw new System.ArgumentException("Invalid pulsar service : " + ServiceName);
			}
			return Port;
		}

	}

}