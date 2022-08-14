// -----------------------------------------------------------------------
// <copyright file="PersistentSnapshotSerializer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using Akka.Actor;
using Akka.Persistence.Serialization.Proto.Msg;
using Akka.Serialization;
using Akka.Util;
using Google.Protobuf;

namespace Akka.Persistence.Pulsar.Serialization
{
    public class PersistentSnapshotSerializer : Serializer
    {
        public PersistentSnapshotSerializer(ExtendedActorSystem system) : base(system)
        {
        }

        public override bool IncludeManifest { get; } = true;

        public override byte[] ToBinary(object obj)
        {
            if (obj is SelectedSnapshot snapshot)
            {
                var snapshotMessage = new SnapshotMessage();
                snapshotMessage.PersistenceId = snapshot.Metadata.PersistenceId;
                snapshotMessage.SequenceNr = snapshot.Metadata.SequenceNr;
                snapshotMessage.TimeStamp =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(snapshot.Metadata.Timestamp);
                snapshotMessage.Payload = GetPersistentPayload(snapshot.Snapshot);
                return snapshotMessage.ToByteArray();
            }

            throw new ArgumentException($"Can't serialize object of type [{obj.GetType()}] in [{GetType()}]");
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            if (type == typeof(SelectedSnapshot))
            {
                var snapshotMessage = SnapshotMessage.Parser.ParseFrom(bytes);

                return new SelectedSnapshot(
                    new SnapshotMetadata(
                        snapshotMessage.PersistenceId,
                        snapshotMessage.SequenceNr,
                        snapshotMessage.TimeStamp.ToDateTime()),
                    GetPayload(snapshotMessage.Payload));
            }

            throw new SerializationException(
                $"Unimplemented deserialization of message with type [{type}] in [{GetType()}]");
        }

        private PersistentPayload GetPersistentPayload(object obj)
        {
            var serializer = system.Serialization.FindSerializerFor(obj);
            var payload = new PersistentPayload();

            if (serializer is SerializerWithStringManifest serializer2)
            {
                var manifest = serializer2.Manifest(obj);
                payload.PayloadManifest = ByteString.CopyFromUtf8(manifest);
            }
            else
            {
                if (serializer.IncludeManifest)
                    payload.PayloadManifest = ByteString.CopyFromUtf8(obj.GetType().TypeQualifiedName());
            }

            payload.Payload = ByteString.CopyFrom(serializer.ToBinary(obj));
            payload.SerializerId = serializer.Identifier;

            return payload;
        }

        private object GetPayload(PersistentPayload payload)
        {
            var manifest = "";
            if (payload.PayloadManifest != null) manifest = payload.PayloadManifest.ToStringUtf8();

            return system.Serialization.Deserialize(payload.Payload.ToByteArray(), payload.SerializerId, manifest);
        }
    }
}