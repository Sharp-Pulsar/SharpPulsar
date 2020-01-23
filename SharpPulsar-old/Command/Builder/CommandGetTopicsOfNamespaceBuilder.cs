using SharpPulsar.Common.PulsarApi;
using static SharpPulsar.Common.PulsarApi.CommandGetTopicsOfNamespace;

namespace SharpPulsar.Command.Builder
{
    public class CommandGetTopicsOfNamespaceBuilder
    {
        private CommandGetTopicsOfNamespace _namespace;
        public CommandGetTopicsOfNamespaceBuilder()
        {
            _namespace = new CommandGetTopicsOfNamespace();
        }
        
        public CommandGetTopicsOfNamespaceBuilder SetMode(Mode mode)
        {
            _namespace.mode = mode;
            return this;
        }
        public CommandGetTopicsOfNamespaceBuilder SetNamespace(string @namespace)
        {
            _namespace.Namespace = @namespace;
            return this;
        }
        public CommandGetTopicsOfNamespaceBuilder SetRequestId(long requestid)
        {
            _namespace.RequestId = (ulong)requestid;
            return this;
        }
        public CommandGetTopicsOfNamespace Build()
        {
            return _namespace;
        }
    }
}
