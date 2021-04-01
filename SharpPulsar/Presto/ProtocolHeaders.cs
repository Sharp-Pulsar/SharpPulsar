using Akka.Util;
using SharpPulsar.Precondition;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Presto
{
	public sealed class ProtocolHeaders
	{
		public static readonly ProtocolHeaders TrinoHeaders = new ProtocolHeaders("Presto");

		private readonly string _name;
		private readonly string _prefix;

		public static ProtocolHeaders CreateProtocolHeaders(string name)
		{
			// canonicalize trino name
			if (TrinoHeaders.ProtocolName.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return TrinoHeaders;
			}
			return new ProtocolHeaders(name);
		}

		private ProtocolHeaders(string name)
		{
			Condition.RequireNonNull(name, "name is null");
			Condition.CheckArgument(name.Length > 0, "name is empty");
			this._name = name;
			this._prefix = "X-" + name + "-";
		}

		public string ProtocolName
		{
			get
			{
				return _name;
			}
		}

		public string RequestUser()
		{
			return _prefix + "User";
		}

		public string RequestSource()
		{
			return _prefix + "Source";
		}

		public string RequestCatalog()
		{
			return _prefix + "Catalog";
		}

		public string RequestSchema()
		{
			return _prefix + "Schema";
		}

		public string RequestPath()
		{
			return _prefix + "Path";
		}

		public string RequestTimeZone()
		{
			return _prefix + "Time-Zone";
		}

		public string RequestLanguage()
		{
			return _prefix + "Language";
		}

		public string RequestTraceToken()
		{
			return _prefix + "Trace-Token";
		}

		public string RequestSession()
		{
			return _prefix + "Session";
		}

		public string RequestRole()
		{
			return _prefix + "Role";
		}

		public string RequestPreparedStatement()
		{
			return _prefix + "Prepared-Statement";
		}

		public string RequestTransactionId()
		{
			return _prefix + "Transaction-Id";
		}

		public string RequestClientInfo()
		{
			return _prefix + "Client-Info";
		}

		public string RequestClientTags()
		{
			return _prefix + "Client-Tags";
		}

		public string RequestClientCapabilities()
		{
			return _prefix + "Client-Capabilities";
		}

		public string RequestResourceEstimate()
		{
			return _prefix + "Resource-Estimate";
		}

		public string RequestExtraCredential()
		{
			return _prefix + "Extra-Credential";
		}

		public string ResponseSetCatalog()
		{
			return _prefix + "Set-Catalog";
		}

		public string ResponseSetSchema()
		{
			return _prefix + "Set-Schema";
		}

		public string ResponseSetPath()
		{
			return _prefix + "Set-Path";
		}

		public string ResponseSetSession()
		{
			return _prefix + "Set-Session";
		}

		public string ResponseClearSession()
		{
			return _prefix + "Clear-Session";
		}

		public string ResponseSetRole()
		{
			return _prefix + "Set-Role";
		}

		public string ResponseAddedPrepare()
		{
			return _prefix + "Added-Prepare";
		}

		public string ResponseDeallocatedPrepare()
		{
			return _prefix + "Deallocated-Prepare";
		}

		public string ResponseStartedTransactionId()
		{
			return _prefix + "Started-Transaction-Id";
		}

		public string ResponseClearTransactionId()
		{
			return _prefix + "Clear-Transaction-Id";
		}
		public static ProtocolHeaders DetectProtocol(Option<string> alternateHeaderName, ISet<string> headerNames)
		{
			Condition.RequireNonNull(alternateHeaderName, "alternateHeaderName is null");
			Condition.RequireNonNull(headerNames, "headerNames is null");

			if (alternateHeaderName.HasValue && !alternateHeaderName.Value.Equals("Trino", StringComparison.OrdinalIgnoreCase))
			{
				if (headerNames.Contains("X-" + alternateHeaderName.Value + "-User"))
				{
					if (headerNames.Contains("X-Trino-User"))
					{
						throw new ProtocolDetectionException("Both Trino and " + alternateHeaderName.Value + " headers detected");
					}
					return CreateProtocolHeaders(alternateHeaderName.Value);
				}
			}

			return TrinoHeaders;
		}
	}
	public class ProtocolDetectionException : Exception
	{
		public ProtocolDetectionException(string message) : base(message)
		{
		}
	}
}
