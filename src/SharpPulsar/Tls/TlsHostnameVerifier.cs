using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using SharpPulsar.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SharpPulsar.Tls
{
    public class TlsHostnameVerifier
    {
		internal sealed class HostNameType
		{

			public static readonly HostNameType IPv4 = new HostNameType("IPv4", InnerEnum.IPv4, 7);
			public static readonly HostNameType IPv6 = new HostNameType("IPv6", InnerEnum.IPv6, 7);
			public static readonly HostNameType DNS = new HostNameType("DNS", InnerEnum.DNS, 2);

			private static readonly List<HostNameType> valueList = new List<HostNameType>();

			static HostNameType()
			{
				valueList.Add(IPv4);
				valueList.Add(IPv6);
				valueList.Add(DNS);
			}

			public enum InnerEnum
			{
				IPv4,
				IPv6,
				DNS
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			internal readonly int SubjectType;

			internal HostNameType(string name, InnerEnum innerEnum, in int subjectType)
			{
				this.SubjectType = subjectType;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public static HostNameType[] values()
			{
				return valueList.ToArray();
			}

			public int ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static HostNameType valueOf(string name)
			{
				foreach (HostNameType enumInstance in valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}
		private readonly ILoggingAdapter _log;

		private readonly DomainParser _publicSuffixMatcher = new DomainParser(new SimpleHttpRuleProvider());
		public TlsHostnameVerifier(ILoggingAdapter log)
		{
			_log = log;
		}

		public bool Verify(string host, X509Certificate2[] certs)
		{
			try
			{
				var x509 = certs[0];
				Verify(host, x509);
				return true;
			}
			catch (Exception ex)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug(ex.Message, ex);
				}
				return false;
			}
		}

		public void Verify(string host, X509Certificate2 cert)
		{
			HostNameType hostType = DetermineHostFormat(host);
			
			var subjectAlts = GetSubjectAltNames(cert);
			if (subjectAlts != null && subjectAlts.Count > 0)
			{
				switch (hostType.innerEnumValue)
				{
					case HostNameType.InnerEnum.IPv4:
						MatchIPAddress(host, subjectAlts);
						break;
					case HostNameType.InnerEnum.IPv6:
						MatchIPv6Address(host, subjectAlts);
						break;
					default:
						MatchDNSName(host, subjectAlts);
						break;
				}
			}
			else
			{
				// CN matching has been deprecated by rfc2818 and can be used
				// as fallback only when no subjectAlts are available
				string cn = cert.GetNameInfo(X509NameType.SimpleName, false);
				if (ReferenceEquals(cn, null))
				{
					throw new Exception("Certificate subject for <" + host + "> doesn't contain " + "a common name and does not have alternative names");
				}
				else
                {
					MatchCN(host, cn);
				}					
			}
		}
		private void MatchIPAddress(string host, IList<string> subjectAlts)
		{
			for (int i = 0; i < subjectAlts.Count; i++)
			{
				var subjectAlt = subjectAlts[i];
				if (host.Equals(subjectAlt))
				{
					return;
				}
			}
			throw new Exception("Certificate for <" + host + "> doesn't match any " + "of the subject alternative names: " + subjectAlts);
		}

		private void MatchIPv6Address(string host, IList<string> subjectAlts)
		{
			string normalisedHost = NormaliseAddress(host);
			for (int i = 0; i < subjectAlts.Count; i++)
			{
				
				var subjectAlt = subjectAlts[i];
				string normalizedSubjectAlt = NormaliseAddress(subjectAlt);
				if (normalisedHost.Equals(normalizedSubjectAlt))
				{
					return;
				}
			}
			throw new Exception("Certificate for <" + host + "> doesn't match any " + "of the subject alternative names: " + subjectAlts);
		}

		private void MatchDNSName(string host, IList<string> subjectAlts)
		{
			string normalizedHost = host.ToLowerInvariant();
			for (int i = 0; i < subjectAlts.Count; i++)
			{				
				var subjectAlt = subjectAlts[i];
				if (MatchIdentityStrict(normalizedHost, subjectAlt))
				{
					return;
				}
			}
			throw new Exception("Certificate for <" + host + "> doesn't match any " + "of the subject alternative names: " + subjectAlts);
		}
		private void MatchCN(string host, string cn)
		{
			string normalizedHost = host.ToLowerInvariant();
			
			string normalizedCn = cn.ToLowerInvariant();
			if (!MatchIdentityStrict(normalizedHost, normalizedCn))
			{
				throw new Exception("Certificate for <" + host + "> doesn't match " + "common name of the certificate subject: " + cn);
			}
		}

		private bool MatchDomainRoot(string host, string domainRoot)
		{
			if (ReferenceEquals(domainRoot, null))
			{
				return false;
			}
			return host.EndsWith(domainRoot, StringComparison.Ordinal) && (host.Length == domainRoot.Length || host[host.Length - domainRoot.Length - 1] == '.');
		}

		private bool MatchIdentity(string host, string identity, bool strict)
		{
			if (host.Contains("."))
			{
				if (!MatchDomainRoot(host, _publicSuffixMatcher.Parse(new Uri(identity)).RegistrableDomain))
				{
					return false;
				}
			}

			// RFC 2818, 3.1. Server Identity
			// "...Names may contain the wildcard
			// character * which is considered to match any single domain name
			// component or component fragment..."
			// Based on this statement presuming only singular wildcard is legal
			int asteriskIdx = identity.IndexOf('*');
			if (asteriskIdx != -1)
			{
				string prefix = identity.Substring(0, asteriskIdx);
				string suffix = identity.Substring(asteriskIdx + 1);
				if (prefix.Length > 0 && !host.StartsWith(prefix, StringComparison.Ordinal))
				{
					return false;
				}
				if (suffix.Length > 0 && !host.EndsWith(suffix, StringComparison.Ordinal))
				{
					return false;
				}
				// Additional sanity checks on content selected by wildcard can be done here
				if (strict)
				{
					string remainder = StringHelper.SubstringSpecial(host, prefix.Length, host.Length - suffix.Length);
					if (remainder.Contains("."))
					{
						return false;
					}
				}
				return true;
			}
			return host.Equals(identity, StringComparison.OrdinalIgnoreCase);
		}

        private bool MatchIdentityStrict(string host, string identity)
		{
			return MatchIdentity(host, identity,  true);
		}

		private HostNameType DetermineHostFormat(string host)
		{
			if (Uri.CheckHostName(host) == UriHostNameType.IPv4)
			{
				return HostNameType.IPv4;
			}
			string s = host;
			if (s.StartsWith("[", StringComparison.Ordinal) && s.EndsWith("]", StringComparison.Ordinal))
			{
				s = host.Substring(1, (host.Length - 1) - 1);
			}
			if (Uri.CheckHostName(s) == UriHostNameType.IPv6)
			{
				return HostNameType.IPv6;
			}
			return HostNameType.DNS;
		}

		private IList<string> GetSubjectAltNames(X509Certificate2 cert)
		{
			try
			{
				foreach (X509Extension extension in cert.Extensions)
				{
					// Create an AsnEncodedData object using the extensions information.
					AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
					if (string.Equals(extension.Oid.FriendlyName, "Subject Alternative Name"))
					{
						return new List<string>(
							asndata.Format(true).Split(new string[] { "\r\n", "DNS Name=" }, StringSplitOptions.RemoveEmptyEntries));
					}

				}
				return new List<string>();
			}
			catch (Exception ex)
			{
				_log.Error(ex.ToString());
				return new List<string>();
			}
		}

		/*
		 * Normalize IPv6 or DNS name.
		 */
		private string NormaliseAddress(string hostname)
		{
			if (ReferenceEquals(hostname, null))
			{
				return hostname;
			}
			try
			{
				return IPAddress.Parse(hostname).ToString();
			}
			catch (Exception ex)
			{ // Should not happen, because we check for IPv6 address above
				_log.Error(ex.ToString());
				return hostname;
			}
		}

	}
}
