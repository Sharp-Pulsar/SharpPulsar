using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Tls;

/*
 * ====================================================================
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 * ====================================================================
 *
 * This software consists of voluntary contributions made by many
 * individuals on behalf of the Apache Software Foundation.  For more
 * information on the Apache Software Foundation, please see
 * <http://www.apache.org/>.
 *
 */

namespace SharpPulsar.Utils
{

	/// <summary>
	/// Default <seealso cref="javax.net.ssl.HostnameVerifier"/> implementation.
	/// 
	/// @since 4.4
	/// </summary>
	public sealed class DefaultHostNameVerifier 
	{

		public sealed class HostNameType
		{

			public static readonly HostNameType IPv4 = new HostNameType("IPv4", InnerEnum.IPv4, 7);
			public static readonly HostNameType IPv6 = new HostNameType("IPv6", InnerEnum.IPv6, 7);
			public static readonly HostNameType Dns = new HostNameType("DNS", InnerEnum.Dns, 2);

			private static readonly IList<HostNameType> ValueList = new List<HostNameType>();

			static HostNameType()
			{
				ValueList.Add(IPv4);
				ValueList.Add(IPv6);
				ValueList.Add(Dns);
			}

			public enum InnerEnum
			{
				IPv4,
				IPv6,
				Dns
			}

			public readonly InnerEnum InnerEnumValue;
			private readonly string _nameValue;
			private readonly int _ordinalValue;
			private static int _nextOrdinal = 0;

			internal readonly int SubjectType;

			public HostNameType(string name, InnerEnum innerEnum, in int subjectType)
			{
				this.SubjectType = subjectType;

				_nameValue = name;
				_ordinalValue = _nextOrdinal++;
				InnerEnumValue = innerEnum;
			}


			public static IList<HostNameType> Values()
			{
				return ValueList;
			}

			public int Ordinal()
			{
				return _ordinalValue;
			}

			public override string ToString()
			{
				return _nameValue;
			}

			public static HostNameType ValueOf(string name)
			{
				foreach (HostNameType enumInstance in HostNameType.ValueList)
				{
					if (enumInstance._nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		private readonly ILogger _log = new LoggerFactory().CreateLogger(typeof(DefaultHostNameVerifier));

		public DefaultHostNameVerifier()
		{
		}

		public bool Verify(in string host, in TlsHandler tlsHandler)
		{
			try
			{
				var x509 = tlsHandler.RemoteCertificate;
				return Verify(host, x509);
			}
			catch (TlsException ex)
			{
				if (_log.IsEnabled(LogLevel.Debug))
				{
					_log.LogDebug(ex.Message, ex);
				}
				return false;
			}
		}

		public bool Verify(in string host, in X509Certificate2 cert)
		{
            var subject = cert.SubjectName.Name;// GetSubjectAltNames(cert.GetNameInfo(X509NameType.));
			if (host.Equals(subject, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
			else
            {
                return false;
            }
		}

		internal static bool MatchDomainRoot(in string host, in string domainRoot)
		{
			if (string.ReferenceEquals(domainRoot, null))
			{
				return false;
			}
			return host.EndsWith(domainRoot, StringComparison.Ordinal) && (host.Length == domainRoot.Length || host[host.Length - domainRoot.Length - 1] == '.');
		}

		internal static HostNameType DetermineHostFormat(in string host)
		{
            IPAddress address;
            if (IPAddress.TryParse(host, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
						return HostNameType.IPv4;
					default:
                    {
							string s = host;
                            if (s.StartsWith("[", StringComparison.Ordinal) && s.EndsWith("]", StringComparison.Ordinal))
                            {
                                s = host.Substring(1, (host.Length - 1) - 1);
                            }

                            if (IPAddress.TryParse(s, out address))
                            {
                                return HostNameType.IPv6;
							}
                            else
                            {
								return HostNameType.Dns;
                            }
                    }
                }
            }
            return HostNameType.Dns;
		}

		/*
		 * Normalize IPv6 or DNS name.
		 */
		internal static string NormaliseAddress(in string hostname)
		{
			if (string.ReferenceEquals(hostname, null))
			{
				return null;
			}
			try
			{
				var inetAddress = Dns.GetHostEntry(hostname);
				return inetAddress.HostName;
			}
			catch (System.Exception)
			{ // Should not happen, because we check for IPv6 address above
				return hostname;
			}
		}
	}

}