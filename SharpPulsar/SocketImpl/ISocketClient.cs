using SharpPulsar.Protocol.Proto;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        void Disconnected();
        IObservable<(BaseCommand command, MessageMetadata metadata, byte[] payload, bool checkSum, short magicNumber)> ReceiveMessageObservable { get; }

        Task SendMessageAsync(byte[] message);
        Task SendMessageAsync(string message);
    }
}
