using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class StartReplayTopic
    {
        public StartReplayTopic(ReaderConfigurationData readerConfigurationData, long @from, long to, long max, Filter filter, bool filtered)
        {
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Filter = filter;
            Filtered = filtered;
        }
        internal StartReplayTopic(ClientConfigurationData clientConfigurationData, ReaderConfigurationData readerConfigurationData, long @from, long to, long max, Filter filter, bool filtered)
        {
            ClientConfigurationData = clientConfigurationData;
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Filter = filter;
            Filtered = filtered;
        }
        public ClientConfigurationData ClientConfigurationData {get;}
        public ReaderConfigurationData ReaderConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
        public Filter Filter { get; }
        public bool Filtered { get; }
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

    public sealed class Filter
    {
        public Filter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}
