using SharpPulsar.Protocol.Proto;
using System;
using System.Buffers;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        void Disconnected();
        IObservable<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> ReceiveMessageObservable { get; }

        void SendMessage(ReadOnlySequence<byte> message);
    }
}
