using Akka.Actor;
using SharpPulsar.Interfaces;
using SharpPulsar.Sql.Client;
using SharpPulsar.Utils;
using System.Collections.Generic;

namespace SharpPulsar.User.Events
{
    public class SqlSourceBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private readonly long _fromMessageId;
        private readonly long _toMessageId;
        private readonly string _brokerWebServiceUrl;
        private readonly ClientOptions _options;
        private readonly HashSet<string> _selectedColumns;
        private readonly ActorSystem _actorSystem;
        
        public SqlSourceBuilder(ActorSystem actorSystem, string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl, ClientOptions options, HashSet<string> selectedColumns)
        {
            _actorSystem = actorSystem;
            _fromMessageId = fromMessageId;
            _toMessageId = toMessageId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _options = options;
            _selectedColumns = selectedColumns;
        }
        public SqlSourceMethod SourceMethod()
        {
            return new SqlSourceMethod(_actorSystem, _tenant, _namespace, _topic, _fromMessageId, _toMessageId, _brokerWebServiceUrl, _options, _selectedColumns);
        }
    }
}
