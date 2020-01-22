using SharpPulsar.Common.PulsarApi;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IProducerStream : IAsyncDisposable
    {
        Task<CommandSendReceipt> Send(ReadOnlySequence<byte> payload);
        Task<CommandSendReceipt> Send(MessageMetadata metadata, ReadOnlySequence<byte> payload);
    }
}
