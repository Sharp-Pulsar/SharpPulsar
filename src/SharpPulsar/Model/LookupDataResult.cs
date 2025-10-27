using Pulsar.Proto;

namespace SharpPulsar.Model
{
    public class LookupDataResult
	{

		public readonly string BrokerUrl;
		public readonly string BrokerUrlTls;
		public readonly string ErrorMessage;
		public readonly int Partitions;
		public readonly bool Authoritative;
		public readonly bool ProxyThroughServiceUrl;
		public readonly bool Redirect;
		public readonly ServerError Error;

		public LookupDataResult(CommandLookupTopicResponse result)
		{
			BrokerUrl = result.BrokerServiceUrl;
			BrokerUrlTls = result.BrokerServiceUrlTls;
			Authoritative = result.Authoritative;
			Redirect = result.Response == CommandLookupTopicResponse.Types.LookupType.Redirect;
			ProxyThroughServiceUrl = result.ProxyThroughServiceUrl;
			Partitions = -1;
			Error = result.Error;
			ErrorMessage = result.Message;
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
