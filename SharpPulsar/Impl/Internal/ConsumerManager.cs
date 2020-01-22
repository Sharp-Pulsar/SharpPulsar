﻿using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Impl.Internal.Interface;
using System;
using System.Buffers;

namespace SharpPulsar.Impl.Internal
{
    public sealed class ConsumerManager : IDisposable
    {
        private readonly IdLookup<IConsumerProxy> _proxies;

        public ConsumerManager() => _proxies = new IdLookup<IConsumerProxy>();

        public bool HasConsumers => !_proxies.IsEmpty();

        public void Outgoing(CommandSubscribe subscribe, IConsumerProxy proxy) => subscribe.ConsumerId = _proxies.Add(proxy);

        public void Dispose()
        {
            foreach (var id in _proxies.AllIds())
            {
                RemoveConsumer(id);
            }
        }

        public void Incoming(CommandMessage message, ReadOnlySequence<byte> data)
        {
            var proxy = _proxies[message.ConsumerId];
            proxy?.Enqueue(new MessagePackage(message.MessageId, data));
        }

        public void Incoming(CommandCloseConsumer command) => RemoveConsumer(command.ConsumerId);

        public void Incoming(CommandActiveConsumerChange command)
        {
            var proxy = _proxies[command.ConsumerId];
            if (proxy is null) return;

            if (command.IsActive)
                proxy.Active();
            else
                proxy.Inactive();
        }

        public void Incoming(CommandReachedEndOfTopic command)
        {
            var proxy = _proxies[command.ConsumerId];
            proxy?.ReachedEndOfTopic();
        }

        public void Remove(ulong consumerId) => _proxies.Remove(consumerId);

        private void RemoveConsumer(ulong consumerId)
        {
            var proxy = _proxies[consumerId];
            if (proxy is null) return;
            proxy.Disconnected();
            _proxies.Remove(consumerId);
        }
    }
}
