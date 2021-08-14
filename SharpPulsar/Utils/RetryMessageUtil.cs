
namespace SharpPulsar.Utils
{
	public class RetryMessageUtil
	{

		public const string SystemPropertyReconsumetimes = "RECONSUMETIMES";
		public const string SystemPropertyDelayTime = "DELAY_TIME";
		public const string SystemPropertyRealTopic = "REAL_TOPIC";
		public const string SystemPropertyRetryTopic = "RETRY_TOPIC";
		public const string SystemPropertyOriginMessageId = "ORIGIN_MESSAGE_IDY_TIME";

		public const int MaxReconsumetimes = 16;
		public const string RetryGroupTopicSuffix = "-RETRY";
		public const string DlqGroupTopicSuffix = "-DLQ";
	}
}
