using Akka.Actor;
using Akka.Serialization;
using System;
using System.IO;

namespace Akka.Persistence.Pulsar
{
    /// <summary>
    ///     INTERNAL API
    /// </summary>
    public sealed class SerializationHelper
    {
        private readonly ActorSystem _actorSystem;

        private readonly Type _persistentRepresentation = typeof(IPersistentRepresentation);
        private readonly Type _snapshotType = typeof(Serialization.Snapshot);

        public SerializationHelper(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public byte[] PersistentToBytes(IPersistentRepresentation message)
        {
            /*
             * Implementation note: Akka.NET caches the serialization lookups internally here,
             * so there's no need to do it again.
             */

            var serializer = _actorSystem.Serialization.FindSerializerForType(_persistentRepresentation);
            return serializer.ToBinary(message);
        }

        public IPersistentRepresentation PersistentFromBytes(byte[] bytes)
        {
            /*
             * Implementation note: Akka.NET caches the serialization lookups internally here,
             * so there's no need to do it again.
             */

            var serializer = _actorSystem.Serialization.FindSerializerForType(_persistentRepresentation);
            var msg = serializer.FromBinary<IPersistentRepresentation>(bytes);
            return msg;
        }
        public Serializer FindSerializerFor(IPersistentRepresentation message)
        {
            return _actorSystem.Serialization.FindSerializerForType(_persistentRepresentation);
        }
        public IPersistentRepresentation PersistentFromBytesWithManifest(byte[] bytes, string manifest)
        {
            /*
             * Implementation note: Akka.NET caches the serialization lookups internally here,
             * so there's no need to do it again.
             */

            var serializer = _actorSystem.Serialization.FindSerializerForType(_persistentRepresentation);
            if (serializer is SerializerWithStringManifest manifestSerializer)
                return (IPersistentRepresentation)manifestSerializer.FromBinary(bytes, manifest);

            return serializer.FromBinary<IPersistentRepresentation>(bytes);
        }

        public byte[] SnapshotToBytes(Serialization.Snapshot snapshot)
        {
            var serializer = _actorSystem.Serialization.FindSerializerForType(_snapshotType);
            return serializer.ToBinary(snapshot);
        }

        public Serialization.Snapshot SnapshotFromBytes(byte[] bytes)
        {
            var serializer = _actorSystem.Serialization.FindSerializerForType(_snapshotType);
            return serializer.FromBinary<Serialization.Snapshot>(bytes);
        }
        
    }
}
