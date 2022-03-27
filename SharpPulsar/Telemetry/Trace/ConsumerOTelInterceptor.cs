using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using OpenTelemetry.Context.Propagation;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Telemetry.Trace
{

    public class ConsumerOTelInterceptor<T> : IConsumerInterceptor<T>
    {
        private enum AcknowledgeType
        {
            Standard,
            Cumulative,
            Negative,
            Timeout
        }
        private readonly Dictionary<IMessageId, Activity> _cache;
        private readonly ILoggingAdapter _log;
        private readonly ActivitySource _activitySource;
        private readonly TextMapPropagator _propagator;
        const string prefix = "ConsumerOTel:";
        public ConsumerOTelInterceptor(string sourceName, ILoggingAdapter log)
        {
            _cache = new Dictionary<IMessageId, Activity>();
            _activitySource = new ActivitySource(sourceName);
            _log = log;
            _propagator = Propagators.DefaultTextMapPropagator;
        }

        private void StopActivitySuccessfully(Activity activity, AcknowledgeType ackType)
        {
            activity.SetTag("messaging.acknowledge_type", ackType).Dispose();
        }
        public IMessage<T> BeforeConsume(IActorRef consumer, IMessage<T> message)
        {
            var topic = string.Empty;
            if (message is TopicMessage<T> m)
            {
                topic = ((TopicMessageId)m.MessageId).TopicName;
            }
            else if (message is Message<T> msg)
            {
                topic = msg.Topic;
            }
            var parentContext = _propagator.Extract(default, message.Properties, ExtractTraceContextFromProperties);
            using var activity = _activitySource.StartActivity(topic, ActivityKind.Consumer, parentContext.ActivityContext);
            if(activity != null)
            {
                activity
                    .SetTag("messaging.system", "pulsar")
                    .SetTag("messaging.destination_kind", "topic")
                    .SetTag("messaging.destination", topic)
                    .SetTag("messaging.consumer_id", $"{consumer.Path.Name} - {consumer.Path.Uid}")
                    .SetTag("messaging.message_id", message.MessageId)
                    .SetTag("messaging.operation", "receive");

                if (activity.IsAllDataRequested)
                {
                    parentContext.Baggage.GetBaggage()
                        .ForEach(kv => activity.AddBaggage(kv.Key, kv.Value));
                }
                _cache[message.MessageId] = activity;   
            }
            return message;
        }
        private void EndActivity(IMessageId messageId, Exception exception, AcknowledgeType acknowledgeType)
        {
            if (_cache.TryGetValue(messageId, out var activity))
            {
                try
                {
                    activity.SetTag("acknowledge.type", acknowledgeType.ToString());
                    if (exception != null)
                    {
                        activity.SetTag("messaging.acknowledge_type", "Error")
                        .SetTag("exception.type", exception.GetType().FullName)
                        .SetTag("exception.message", exception.Message)
                        .SetTag("exception.stacktrace", exception.StackTrace)
                        .Dispose();
                    }
                    else
                    {
                        StopActivitySuccessfully(activity, acknowledgeType);
                    }
                }
                finally
                {
                    _cache.Remove(messageId);
                }
            }
        }
        private IEnumerable<string> ExtractTraceContextFromProperties(IDictionary<string,string> props, string key)
        {
            try
            {
                if (props.TryGetValue(key, out var value))
                {
                    return new[] { value };
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }
        public void Dispose()
        {
            
        }

        public void OnAcknowledge(IActorRef consumer, IMessageId messageId, Exception exception)
        {
            EndActivity(messageId, exception, AcknowledgeType.Standard);
        }

        public void OnAcknowledgeCumulative(IActorRef consumer, IMessageId messageId, Exception exception)
        {
            EndActivity(messageId, exception, AcknowledgeType.Cumulative);
        }

        public void OnAckTimeoutSend(IActorRef consumer, ISet<IMessageId> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                EndActivity(messageId, null, AcknowledgeType.Timeout);
            }
        }

        public void OnNegativeAcksSend(IActorRef consumer, ISet<IMessageId> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                EndActivity(messageId, null, AcknowledgeType.Negative);
            }
        }
    }
}
