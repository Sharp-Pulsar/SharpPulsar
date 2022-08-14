using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace Akka.Persistence.Pulsar
{
    internal class JournalHelper
    {
        private readonly ActorSystem _system;
        private readonly Akka.Serialization.Serialization _serialization;
        private readonly Akka.Serialization.Serializer _serializer;

        public JournalHelper(ActorSystem system)
        {
            _system = system;
            _serialization = system.Serialization;
            _serializer = _serialization.FindSerializerForType(typeof(Persistent));
        }

        public string KeyPrefix { get; }

        public byte[] PersistentToBytes(IPersistentRepresentation message)
        {
            return _serialization.Serialize(message);
        }

        public IPersistentRepresentation PersistentFromBytes(byte[] bytes)
        {
            var p = (IPersistentRepresentation)_serialization.Deserialize(bytes, _serializer.Identifier,
                typeof(Persistent));
            return p;
        }

    }
}
