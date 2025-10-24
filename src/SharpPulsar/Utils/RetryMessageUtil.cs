
using DotNetty.Common.Utilities;

namespace SharpPulsar.Utils
{
	public class RetryMessageUtil
	{

		public const string SystemPropertyReconsumetimes = "RECONSUMETIMES";
		public const string SystemPropertyDelayTime = "DELAY_TIME";
		public const string SystemPropertyRealTopic = "REAL_TOPIC";
		public const string SystemPropertyRetryTopic = "RETRY_TOPIC";
        public const string SystemPropertyRealSubscription = "REAL_SUBSCRIPTION";
		public const string SystemPropertyOriginMessageId = "ORIGIN_MESSAGE_IDY_TIME";


        public const string PropertyOriginMessageId = "ORIGIN_MESSAGE_ID";

		public const int MaxReconsumetimes = 16;
		public const string RetryGroupTopicSuffix = "-RETRY";
		public const string DlqGroupTopicSuffix = "-DLQ";

        public static string GetRetryTopic(string topic, string subscription)
        {
            return topic + "-" + subscription + RetryGroupTopicSuffix;
        }

        public static string GetDLQTopic(string topic, string subscription)
        {
            return topic + "-" + subscription + DlqGroupTopicSuffix;
        }
    }
}
