using System;
using System.Buffers;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        void Disconnected();
        IObservable<ReadOnlySequence<byte>> ReceiveMessageObservable { get; }

        Task SendMessageAsync(byte[] message);
        Task SendMessageAsync(string message);
    }
}
