﻿using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Impl.Internal.Interface;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using static SharpPulsar.Exception.PulsarClientException;

namespace SharpPulsar.Impl.Internal
{
    public sealed class NotReadyStream : IConsumerStream, IProducerStream
    {
        public ValueTask DisposeAsync() => new ValueTask();

        public ValueTask<Message> Receive(CancellationToken cancellationToken) => throw GetException();

        public Task Send(CommandAck command) => throw GetException();

        public Task<CommandSuccess> Send(CommandUnsubscribe command) => throw GetException();

        public Task<CommandSuccess> Send(CommandSeek command) => throw GetException();

        public Task<CommandGetLastMessageIdResponse> Send(CommandGetLastMessageId command) => throw GetException();

        public Task<CommandSendReceipt> Send(byte[] payload) => throw GetException();

        public Task<CommandSendReceipt> Send(ReadOnlyMemory<byte> payload) => throw GetException();

        public Task<CommandSendReceipt> Send(ReadOnlySequence<byte> payload) => throw GetException();

        public Task<CommandSendReceipt> Send(MessageMetadata metadata, byte[] payload) => throw GetException();

        public Task<CommandSendReceipt> Send(MessageMetadata metadata, ReadOnlyMemory<byte> payload) => throw GetException();

        public Task<CommandSendReceipt> Send(MessageMetadata metadata, ReadOnlySequence<byte> payload) => throw GetException();

        private Exception GetException() => new StreamNotReadyException();
    }
}
