using System;
using System.Text.Json;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Messages;

namespace SharpPulsar.Handlers
{
    public class DefaultProducerListener : IProducerEventListener
    {
        private readonly Action<string> _receipts;
        private readonly Action<object> _logs;

        public DefaultProducerListener(Action<object> log, Action<string> receipts)
        {
            _receipts = receipts;
            _logs = log;
        }

        public void MessageSent(SentReceipt receipt)
        {
            var json = JsonSerializer.Serialize(receipt);
            _receipts.Invoke(json);
            var s = $"Receipt Added [{json}]";
            Log(s);
        }

        public void Log(object log)
        {
            _logs.Invoke(log);
        }

    }
}
