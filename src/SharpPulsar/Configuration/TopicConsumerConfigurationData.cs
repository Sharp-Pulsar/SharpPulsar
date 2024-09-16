
using System.Text.RegularExpressions;

namespace SharpPulsar.Configuration
{
    public class TopicConsumerConfigurationData
    {

        private ITopicNameMatcher _topicNameMatcher;
        private int _priorityLevel;

        public TopicConsumerConfigurationData()
        {     
            
        }
        public TopicConsumerConfigurationData(ITopicNameMatcher topicNameMatcher, int priorityLevel)
        {
            _topicNameMatcher = topicNameMatcher;   
            _priorityLevel = priorityLevel;
        }
        public static TopicConsumerConfigurationData OfTopicsPattern(string topicsPattern, int priorityLevel)
        {
            return Of(new TopicsPattern(topicsPattern), priorityLevel);
        }
        public int PriorityLevel()
        {
            return _priorityLevel;  
        }
        public static TopicConsumerConfigurationData OfTopicsPattern<T1>(string topicsPattern, ConsumerConfigurationData<T1> conf)
        {
            return OfTopicsPattern(topicsPattern, conf.PriorityLevel);
        }

        public static TopicConsumerConfigurationData OfTopicName(string topicName, int priorityLevel)
        {
            return Of(new TopicName(topicName), priorityLevel);
        }

        public static TopicConsumerConfigurationData OfTopicName<T1>(string topicName, ConsumerConfigurationData<T1> conf)
        {
            return OfTopicName(topicName, conf.PriorityLevel);
        }

        internal static TopicConsumerConfigurationData Of(ITopicNameMatcher topicNameMatcher, int priorityLevel)
        {
            return new TopicConsumerConfigurationData(topicNameMatcher, priorityLevel);
        }

        public ITopicNameMatcher GetTopicNameMatcher()
        {
            return _topicNameMatcher;
        }
        public interface ITopicNameMatcher
        {
            bool Matches(string topicName);
        }

        public class TopicsPattern : ITopicNameMatcher
        {
            public TopicsPattern(string pattern)
            {
               _topicsPattern = new Regex(pattern);
            }

            
            private readonly Regex _topicsPattern;

            public virtual bool Matches(string topicName)
            {
                return _topicsPattern.Matches(topicName).Count > 0;
            }
        }

        public class TopicName : ITopicNameMatcher
        {
          
            public TopicName(string topicName)
            {
               _topicName = topicName;
            }

            private readonly string _topicName;

            public virtual bool Matches(string topicName)
            {
                return _topicName.Equals(topicName);
            }
        }
    }

}
