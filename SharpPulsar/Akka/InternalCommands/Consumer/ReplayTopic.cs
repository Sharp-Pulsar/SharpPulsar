using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class StartReplayTopic
    {
        internal StartReplayTopic(ClientConfigurationData clientConfigurationData, ReaderConfigurationData readerConfigurationData, string adminUrl, long @from, long to, long max, Tag tag, bool tagged)
        {
            ClientConfigurationData = clientConfigurationData;
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Tag = tag;
            Tagged = tagged;
            AdminUrl = adminUrl;
        }
        public ClientConfigurationData ClientConfigurationData {get;}
        public ReaderConfigurationData ReaderConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
        public Tag Tag { get; }
        public bool Tagged { get; }
        public string AdminUrl { get; }
    }
    public sealed class ReplayTopic
    {
        public ReplayTopic(ReaderConfigurationData readerConfigurationData, string adminUrl, long @from, long to, long max, Tag tag, bool tagged)
        {
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Tag = tag;
            Tagged = tagged;
            AdminUrl = adminUrl;
        }
        public ReaderConfigurationData ReaderConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
        public Tag Tag { get; }
        public bool Tagged { get; }
        public string AdminUrl { get; }
    }
    public sealed class GetNumberOfEntries 
    {
        public GetNumberOfEntries(string topic, string server)
        {
            Topic = topic;
            Server = server;
        }
        internal GetNumberOfEntries(TopicName topic, string server)
        {
            TopicName = topic;
            Server = server;
        }
        public string Topic { get; }
        public string Server { get; }
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

    public sealed class Tag
    {
        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}
