using SharpPulsar.Protocol.Proto;
using System;
using System.Buffers;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        void Disconnected();
        IObservable<(BaseCommand command, MessageMetadata metadata, ReadOnlySequence<byte> payload, bool checkSum, short magicNumber)> ReceiveMessageObservable { get; }

        void SendMessage(ReadOnlySequence<byte> message);
    }
}
