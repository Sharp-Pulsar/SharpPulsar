using SharpPulsar.Sql.Client;
using System;
using System.Collections.Generic;

namespace SharpPulsar.User.Events
{
    internal class SqlSourceBuilder : ISourceBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private readonly long _fromSequenceId;
        private readonly long _toSequenceId;
        private readonly string _brokerWebServiceUrl;
        private readonly ClientOptions _options;
        private readonly HashSet<string> _selectedColumns;
        
        public SqlSourceBuilder(string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ClientOptions options, HashSet<string> selectedColumns)
        {
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _options = options;
            _selectedColumns = selectedColumns;
        }
        public ISourceMethodBuilder SourceMethod()
        {
            throw new NotImplementedException();
        }
    }
}
