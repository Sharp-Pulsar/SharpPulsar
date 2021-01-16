using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Model
{
	public class LookupDataResult
	{

		public readonly string BrokerUrl;
		public readonly string BrokerUrlTls;
		public readonly int Partitions;
		public readonly bool Authoritative;
		public readonly bool ProxyThroughServiceUrl;
		public readonly bool Redirect;

		public LookupDataResult(CommandLookupTopicResponse result)
		{
			BrokerUrl = result.brokerServiceUrl;
			BrokerUrlTls = result.brokerServiceUrlTls;
			Authoritative = result.Authoritative;
			Redirect = result.Response == CommandLookupTopicResponse.LookupType.Redirect;
			ProxyThroughServiceUrl = result.ProxyThroughServiceUrl;
			Partitions = -1;
		}

		public LookupDataResult(int partitions) : base()
		{
			Partitions = partitions;
			BrokerUrl = null;
			BrokerUrlTls = null;
			Authoritative = false;
			ProxyThroughServiceUrl = false;
			Redirect = false;
		}

	}

}
