using SharpPulsar.Protocol.Proto;
using System;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        void Disconnected();
        IObservable<(BaseCommand command, MessageMetadata metadata, byte[] payload, bool checkSum, short magicNumber)> ReceiveMessageObservable { get; }

        void SendMessage(byte[] message);
    }
}
