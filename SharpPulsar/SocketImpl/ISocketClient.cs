using System;
using System.Buffers;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    public interface ISocketClient: IDisposable
    {
        string RemoteConnectionId { get; }
        IObservable<ReadOnlySequence<byte>> RevicedObservable { get; }

        Task SendMessageAsync(byte[] message);
        Task SendMessageAsync(string message);
    }
}
