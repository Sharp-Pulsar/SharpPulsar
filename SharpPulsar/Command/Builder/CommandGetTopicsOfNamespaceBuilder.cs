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
        private CommandGetTopicsOfNamespaceBuilder(CommandGetTopicsOfNamespace @namespace)
        {
            _namespace = @namespace;
        }
        public CommandGetTopicsOfNamespaceBuilder SetMode(Mode mode)
        {
            _namespace.mode = mode;
            return new CommandGetTopicsOfNamespaceBuilder(_namespace);
        }
        public CommandGetTopicsOfNamespaceBuilder SetNamespace(string @namespace)
        {
            _namespace.Namespace = @namespace;
            return new CommandGetTopicsOfNamespaceBuilder(_namespace);
        }
        public CommandGetTopicsOfNamespaceBuilder SetRequestId(long requestid)
        {
            _namespace.RequestId = (ulong)requestid;
            return new CommandGetTopicsOfNamespaceBuilder(_namespace);
        }
        public CommandGetTopicsOfNamespace Build()
        {
            return _namespace;
        }
    }
}
