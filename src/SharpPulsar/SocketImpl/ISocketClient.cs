using SharpPulsar.Protocol.Proto;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }

        event Action OnConnect;

        void Disconnected();
        IObservable<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> ReceiveMessageObservable { get; }

        ValueTask SendMessage(ReadOnlySequence<byte> message);
    }
}
