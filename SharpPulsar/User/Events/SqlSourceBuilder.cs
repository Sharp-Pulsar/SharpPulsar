using Akka.Actor;
using SharpPulsar.Sql.Client;
using System.Collections.Generic;

namespace SharpPulsar.User.Events
{
    public class SqlSourceBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private readonly long _fromSequenceId;
        private readonly long _toSequenceId;
        private readonly string _brokerWebServiceUrl;
        private readonly ClientOptions _options;
        private readonly HashSet<string> _selectedColumns;
        private readonly ActorSystem _actorSystem;
        
        public SqlSourceBuilder(ActorSystem actorSystem, string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ClientOptions options, HashSet<string> selectedColumns)
        {
            _actorSystem = actorSystem;
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _options = options;
            _selectedColumns = selectedColumns;
        }
        public SqlSourceMethod SourceMethod()
        {
            return new SqlSourceMethod(_actorSystem, _tenant, _namespace, _topic, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, _options, _selectedColumns);
        }
    }
}
