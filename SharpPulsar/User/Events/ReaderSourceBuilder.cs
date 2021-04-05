using SharpPulsar.Configuration;
using System;

namespace SharpPulsar.User.Events
{
    internal class ReaderSourceBuilder<T> : ISourceBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromSequenceId;
        private long _toSequenceId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;
        public ReaderSourceBuilder(string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
        {            
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _conf = readerConfigBuilder;
        }

        public ISourceMethodBuilder SourceMethod()
        {
            return new ReaderSourceMethod<T>(_tenant, _namespace, _topic, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, _conf);
        }
    }
}
