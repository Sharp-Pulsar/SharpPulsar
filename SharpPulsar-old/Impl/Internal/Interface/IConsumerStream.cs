using SharpPulsar.Common.PulsarApi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IConsumerStream : IAsyncDisposable
    {
        Task Send(CommandAck command);
        Task<CommandSuccess> Send(CommandUnsubscribe command);
        Task<CommandSuccess> Send(CommandSeek command);
        Task<CommandGetLastMessageIdResponse> Send(CommandGetLastMessageId command);
        ValueTask<Message> Receive(CancellationToken cancellationToken = default);
    }
}
