using System;
using SharpPulsar.Configuration;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct StartReplayTopic<T>
    {
        internal StartReplayTopic(ClientConfigurationData clientConfigurationData, ReaderConfigurationData<T> readerConfigurationData, string adminUrl, long @from, long to, long max, Tag tag, bool tagged, SourceType source)
        {
            ClientConfigurationData = clientConfigurationData;
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Tag = tag;
            Tagged = tagged;
            Source = source;
            AdminUrl = adminUrl;
        }
        public ClientConfigurationData ClientConfigurationData {get;}
        public ReaderConfigurationData<T> ReaderConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
        public Tag Tag { get; }
        public bool Tagged { get; }
        public string AdminUrl { get; }
        public SourceType Source { get; }
    }
    public readonly record struct ReplayTopic<T>
    {
        public ReplayTopic(ReaderConfigurationData<T> readerConfigurationData, string adminUrl, long @from, long to, long max, Tag tag, bool tagged, SourceType source)
        {
            ReaderConfigurationData = readerConfigurationData;
            From = @from;
            To = to;
            Max = max;
            Tag = tag;
            Tagged = tagged;
            Source = source;
            AdminUrl = adminUrl;
        }
        public ReaderConfigurationData<T> ReaderConfigurationData { get; }
        public long From { get; }
        public long To { get; }
        public long Max { get; }
        public Tag Tag { get; }
        public bool Tagged { get; }
        public string AdminUrl { get; }
        public SourceType Source { get; }
    }
    public readonly record struct GetNumberOfEntries 
    {
        public GetNumberOfEntries(string topic, string server, SourceType source)
        {
            Topic = topic;
            Server = server;
            Source = source;
        }
        public string Topic { get; }
        public string Server { get; }
        public SourceType Source { get; }

    }
    public readonly record struct TopicEntries 
    {
        public TopicEntries(string topic, long? max, long? totalEntries, int totalNumberOfTopics)
        {
            Topic = topic;
            Max = max;
            TotalEntries = totalEntries;
            TotalNumberOfTopics = totalNumberOfTopics;
        }
        public string Topic { get; }
        public long? Max { get; }
        public long? TotalEntries { get; }
        public int TotalNumberOfTopics { get; }
    }
    public readonly record struct NextPlay 
    {
        public NextPlay(string topic, long max, long @from, long to, SourceType source, bool tagged = false)
        {
            Topic = topic;
            Max = max;
            From = @from;
            To = to;
            Source = source;
            Tagged = tagged;
        }
        public string Topic { get; }
        public long Max { get; }
        public long From { get; }
        public long To { get; }
        public bool Tagged { get; }
        public SourceType Source { get; }
    }

    public readonly record struct Tag
    {
        public Tag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tag key cannot be null/empty");
            
            if(string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tag value cannot be null/empty");

            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }

    public enum SourceType
    {
        Pulsar,
        Presto
    }
}
