using Akka.Actor;
using SharpPulsar.Admin.v2;
using SharpPulsar.EventSource.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using static System.Net.WebRequestMethods;

namespace SharpPulsar.Events
{
    public class SqlSource<T>
    {
        private readonly IActorRef _eventSource;
        private readonly HttpClient _httpclient;
        private readonly PulsarAdminRESTAPIClient _admin;
        private readonly BufferBlock<IEventEnvelope> _buffer;

        public SqlSource(string brokerWebServiceUrl, BufferBlock<IEventEnvelope> buffer, IActorRef sourceActor)
        {
            _buffer = buffer;
            _eventSource = sourceActor;
            _httpclient = new HttpClient
            {
                BaseAddress = new Uri(brokerWebServiceUrl)
            };
            _admin = new PulsarAdminRESTAPIClient(_httpclient);
        }
        public IList<string> Topics(IEventTopics message)
        {
            var response = _admin.GetTopicsAsync(message.Tenant, message.Namespace, Mode.ALL, false).GetAwaiter().GetResult();
           
            return response.ToList();
        }
        /// <summary>
        /// Reads existing events and future events from Presto
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventEnvelope></returns>
        public IEnumerable<IEventEnvelope> Events(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_buffer.TryReceive(out var msg))
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
        public IEnumerable<IEventEnvelope> CurrentEvents(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_buffer.TryReceive(out var msg))
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
        public async IAsyncEnumerable<IEventEnvelope> Events(TimeSpan timeout, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var msg = await _buffer.ReceiveAsync(timeout);
                if (msg != null)
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
        public async IAsyncEnumerable<IEventEnvelope> CurrentEvents(TimeSpan timeout, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var msg = await _buffer.ReceiveAsync(timeout);
                if (msg != null)
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
