
using SharpPulsar.API;

namespace SharpPulsar.Builder
{
    internal class DeadLetterProducerBuilderContext : IDeadLetterProducerBuilderContext
    {
        private readonly string defaultTopicName;
        private readonly string inputTopicName;
        private readonly string inputTopicSubscriptionName;
        private readonly string inputTopicConsumerName;

        public string DefaultTopicName
        {
            get
            {
                return defaultTopicName;
            }
        }

        public string InputTopicName
        {
            get
            {
                return inputTopicName;
            }
        }

        public string InputTopicSubscriptionName
        {
            get
            {
                return inputTopicSubscriptionName;
            }
        }

        public string InputTopicConsumerName
        {
            get
            {
                return inputTopicConsumerName;
            }
        }
    }

}
