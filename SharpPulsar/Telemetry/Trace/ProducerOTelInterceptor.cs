using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Interceptor;

namespace SharpPulsar.Telemetry.Trace
{
    public class ProducerOTelInterceptor<T> : IProducerInterceptor<T>
    {
        private readonly Dictionary<string, Activity> _cache;
        private readonly ILoggingAdapter _log;
        private readonly ActivitySource _activitySource;
        private readonly TextMapPropagator _propagator;
        const string prefix = "OtelProducerInterceptor:";
        const string _activityKey = "traceparent";
        public ProducerOTelInterceptor(string sourceName, ILoggingAdapter log)
        {
            _cache = new Dictionary<string, Activity>();
            _log = log;
            _activitySource = new ActivitySource(sourceName);   
            _propagator = Propagators.DefaultTextMapPropagator;
        }
        private void InjectTraceContextIntoProperties(IDictionary<string,string> props, string key, string value)
        {
            try
            {
                if (props == null)
                {
                    props = new Dictionary<string, string>();
                }

                props[key] = value;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to inject trace context.{ex}");
            }
        }
        public IMessage<T> BeforeSend(IActorRef producer, IMessage<T> message)
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
            var mutableDict = new Dictionary<string, string>(message.Properties.Count);
            message.Properties.ForEach((kv)=> mutableDict.Add(kv.Key, kv.Value));
            void AddToCache(Activity activity)
            {
                if(_cache.TryAdd(mutableDict[_activityKey], activity))
                {
                    return; 
                }
                else
                {
                    activity.SetTag("messaging.acknowledge_type", "Duplicate");
                    activity.Dispose();
                    _log.Warning($"{prefix} Duplicate activity detected");
                }
            }
            using var activity = _activitySource.StartActivity(topic+" send", ActivityKind.Producer);

            if (activity == null)
                return message;

            activity.SetTag("messaging.system", "pulsar")
                    .SetTag("messaging.destination_kind", "topic")
                    .SetTag("messaging.destination", topic)
                    .SetTag("messaging.operation", "send")
                    .SetTag("messaging.producer_id", $"{producer.Path.Name} - {producer.Path.Uid}");

            if (activity.IsAllDataRequested)
            {
                // It is highly recommended to check activity.IsAllDataRequested,
                // before populating any tags which are not readily available.
                // IsAllDataRequested is the same as Span.IsRecording and will be false
                // when samplers decide to not record the activity,
                // and this can be used to avoid any expensive operation to retrieve tags.
                //https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md
                //https://github.com/open-telemetry/opentelemetry-dotnet/blob/a25741030f05c60c85be102ce7c33f3899290d49/examples/MicroserviceExample/Utils/Messaging/MessageSender.cs#L102
                var contextToInject = activity.Context;
                _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), mutableDict, InjectTraceContextIntoProperties);
                AddToCache(activity);
                mutableDict.ForEach((kv) => message.Properties.Add(kv.Key,kv.Value));
                return message;
            }
            AddToCache(activity);
            return message;
        }

        public bool Eligible(IMessage<T> message)
        {
            return true;
        }

        public void OnSendAcknowledgement(IActorRef producer, IMessage<T> message, IMessageId msgId, Exception exception)
        {
            if (message.Properties.TryGetValue(_activityKey, out var activityId))
            {
                if (_cache.TryGetValue(activityId, out var activity))
                {
                    if (exception == null)
                    {
                        activity
                               .SetTag("messaging.acknowledge_type", "Success")
                               .SetTag("messaging.message_id", message.MessageId)
                               .Dispose();
                    }
                    else
                    {
                        //https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/exceptions.md                                             
                        activity
                            .SetTag("messaging.acknowledge_type", "Error")
                            .SetTag("exception.type", exception.GetType().FullName)
                            .SetTag("exception.message", exception.Message)
                            .SetTag("exception.stacktrace", exception.StackTrace)
                            .Dispose();
                    }
                }
                else
                    _log.Warning($"{prefix} Can't find start of activity for msgId={message.MessageId}");
            }
            else
                _log.Warning($"{prefix} activity id is missing for msgId={message.MessageId}");
        }
    }
}
