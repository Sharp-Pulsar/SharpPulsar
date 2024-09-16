
using SharpPulsar.Configuration;
using static SharpPulsar.Configuration.TopicConsumerConfigurationData;

namespace SharpPulsar.Test
{
    public class TopicConsumerConfigurationDataTest
    {
        [Fact]
        public void TestOfFactoryMethod()
        {
            var topicConsumerConfigurationData = OfTopicName("foo", 1);

            Assert.True(topicConsumerConfigurationData.GetTopicNameMatcher().Matches("foo"));
            Assert.Equal(1, topicConsumerConfigurationData.PriorityLevel());

            var pattern = new TopicConsumerConfigurationData(new TopicsPattern("^foo$"), 1);
            Assert.True(pattern.GetTopicNameMatcher().Matches("foo"));
            Assert.Equal(1, pattern.PriorityLevel());
        }
        [Fact]
        public void TestOfDefaultFactoryMethod()
        {
            var consumerConfigurationData = new ConsumerConfigurationData<object>();
            consumerConfigurationData.PriorityLevel = 1;
            var topicConsumerConfigurationData = OfTopicName("foo", consumerConfigurationData);

            Assert.True(topicConsumerConfigurationData.GetTopicNameMatcher().Matches("foo"));
            Assert.Equal(1, topicConsumerConfigurationData.PriorityLevel());
        }

        public static object[][] TopicNameMatch()
        {
            return new object[][]
            {
                new object[] {"foo", true},
                new object[] {"bar", false}
            };
        }

        [Theory]
        [MemberData(nameof(TopicNameMatch))]    
        public void TestTopicNameMatch(string topicName, bool expectedMatch)
        {
            var topicConsumerConfigurationData = OfTopicsPattern("^foo$", 1);
            Assert.Equal(expectedMatch, topicConsumerConfigurationData.GetTopicNameMatcher().Matches(topicName));
        }


        [Fact]
        public void TestTopicNameMatchNullTopicName()
        {
            Assert.False(OfTopicName("foo", 1).GetTopicNameMatcher().Matches(null));
        }

    }
}
