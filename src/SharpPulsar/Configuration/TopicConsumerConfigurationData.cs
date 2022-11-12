using System;
using System.Text.RegularExpressions;
namespace SharpPulsar.Configuration
{
    [Serializable]
    public class TopicConsumerConfigurationData<T>
    {
        private const long SerialVersionUID = 1L;

        private ITopicNameMatcher _topicNameMatcher;
        private int _priorityLevel;
        public TopicConsumerConfigurationData(ITopicNameMatcher topicNameMatcher, int priorityLevel)
        {
            _topicNameMatcher = topicNameMatcher;
            _priorityLevel = priorityLevel;
        }
    
        public static TopicConsumerConfigurationData<T> OfTopicsPattern(Regex topicsPattern, int priorityLevel)
        {
            return Of(new TopicNameMatcherTopicsPattern(topicsPattern), priorityLevel);
        }
        public static TopicConsumerConfigurationData<T> OfTopicsPattern<T1>(Regex topicsPattern, ConsumerConfigurationData<T1> conf)
        {
            return OfTopicsPattern(topicsPattern, conf.PriorityLevel);
        }

        public static TopicConsumerConfigurationData<T> OfTopicName(string topicName, int priorityLevel)
        {
            return Of(new TopicNameMatcherTopicName(topicName), priorityLevel);
        }

         public static TopicConsumerConfigurationData<T> OfTopicName(string topicName, ConsumerConfigurationData<T> conf)
        {
            return OfTopicName(topicName, conf.PriorityLevel);
        }

        internal static TopicConsumerConfigurationData<T> Of(ITopicNameMatcher topicNameMatcher, int priorityLevel)
        {
            return new TopicConsumerConfigurationData<T>(topicNameMatcher, priorityLevel);
        }

        public interface ITopicNameMatcher
        {
            bool Matches(string topicName);
        }

        [Serializable]
        public class TopicNameMatcherTopicsPattern : ITopicNameMatcher
        {
            private readonly TopicConsumerConfigurationData<T> _outerInstance;

            public TopicNameMatcherTopicsPattern(TopicConsumerConfigurationData<T> outerInstance)
            {
                _outerInstance = outerInstance;

            }

            internal readonly Regex TopicsPattern;

            public virtual bool Matches(string topicName)
            {
                return TopicsPattern.Match(topicName).Success;
            }
        }

        [Serializable]
        public class TopicNameMatcherTopicName : ITopicNameMatcher
        {
            private readonly TopicConsumerConfigurationData<T> _outerInstance;

            public TopicNameMatcherTopicName(TopicConsumerConfigurationData<T>   outerInstance)
            {
                _outerInstance = outerInstance;
            }

            internal readonly string TopicName;

            public virtual bool Matches(string topicName)
            {
                return TopicName.Equals(topicName);
            }
        }
    }
}
