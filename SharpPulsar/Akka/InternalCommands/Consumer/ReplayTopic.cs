using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class StartReplayTopic
    {
        public StartReplayTopic(ConsumerConfigurationData consumerConfigurationData, long @from, long to, long max)
        {
            ConsumerConfigurationData = consumerConfigurationData;
            From = @from;
            To = to;
            Max = max;
        }
        internal StartReplayTopic(ClientConfigurationData clientConfigurationData, ConsumerConfigurationData consumerConfigurationData, long @from, long to, long max)
        {
            ClientConfigurationData = clientConfigurationData;
            ConsumerConfigurationData = consumerConfigurationData;
            From = @from;
            To = to;
            Max = max;
        }
        public ClientConfigurationData ClientConfigurationData {get;}
        public ConsumerConfigurationData ConsumerConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
    }
    public sealed class GetNumberOfEntries 
    {
        public GetNumberOfEntries(string topic)
        {
            Topic = topic;
        }
        internal GetNumberOfEntries(TopicName topic)
        {
            TopicName = topic;
        }
        public string Topic { get; }
        public TopicName TopicName { get; }
    }
    public sealed class NumberOfEntries 
    {
        public NumberOfEntries(string topic, long? max)
        {
            Topic = topic;
            Max = max;
        }
        public string Topic { get; }
        public long? Max { get; }
    }
    public sealed class NextPlay 
    {
        public NextPlay(string topic, long max, long @from, long to)
        {
            Topic = topic;
            Max = max;
            From = @from;
            To = to;
        }
        public string Topic { get; }
        public long Max { get; }
        public long From { get; }
        public long To { get; }
    }
}
