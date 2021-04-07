using Akka.Actor;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.User.Events
{
    public class EventSource<T>
    {
        private IActorRef _eventSource;
        private HttpClient _httpclient;
        private readonly Admin _admin;
        private readonly BufferBlock<T> _buffer;
        private readonly BufferBlock<IMessage<T>> _readerBuffer;

        public EventSource(string brokerWebServiceUrl, BufferBlock<T> buffer, IActorRef sourceActor)
        {
            _buffer = buffer;
            _eventSource = sourceActor;
            _httpclient = new HttpClient();
            _admin = new Admin(brokerWebServiceUrl, _httpclient, true);
        }
        public EventSource(string brokerWebServiceUrl, BufferBlock<IMessage<T>> buffer, IActorRef sourceActor)
        {
            _readerBuffer = buffer;
            _eventSource = sourceActor;
            _httpclient = new HttpClient();
            _admin = new Admin(brokerWebServiceUrl, _httpclient, true);
        }
        public IList<string> Topics(IEventTopics message)
        {
            var response = _admin.GetTopics(message.Tenant, message.Namespace, "ALL");
            var statusCode = response.Response.StatusCode;
            if (response == null)
                return new List<string>();
            return response.Body;
        }
        /// <summary>
        /// Reads existing events and future events from Presto
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventEnvelope></returns>
        public IEnumerable<IEventEnvelope> SourceEventsFromPresto(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PrestoEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
            }
        }

        /// <summary>
        /// Reads existing events from Presto
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventEnvelope></returns>
        public IEnumerable<IEventEnvelope> SourceCurrentEventsFromPresto(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PrestoEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Reads existing events and future events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<EventMessage> SourceEventsFromReader(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PulsarEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
            }
        }

        /// <summary>
        /// Reads existing events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<EventMessage> SourceCurrentEventsFromReader(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PulsarEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
