using Akka.Actor;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.User.Events
{
    public class ReaderSource<T>
    {
        private IActorRef _eventSource;
        private HttpClient _httpclient;
        private readonly Admin _admin;
        private readonly BufferBlock<IMessage<T>> _readerBuffer;
        public ReaderSource(string brokerWebServiceUrl, BufferBlock<IMessage<T>> buffer, IActorRef sourceActor)
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
        /// Reads existing events and future events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<T> Events(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_readerBuffer.TryReceive(out var msg))
                {
                    yield return msg.Value;

                }
            }
        }
        /// <summary>
        /// Reads existing events and future events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public async IAsyncEnumerable<T> Events(TimeSpan timeout, [EnumeratorCancellation]CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var msg = await _readerBuffer.ReceiveAsync(timeout);
                if (msg != null)
                {
                    yield return msg.Value;
                }
            }
        }

        /// <summary>
        /// Reads existing events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<T> CurrentEvents(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_readerBuffer.TryReceive(out var msg))
                {
                    yield return msg.Value;
                }
                else
                {
                    break;
                }
            }
        }
        /// <summary>
        /// Reads existing events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public async IAsyncEnumerable<T> CurrentEvents(TimeSpan timeout, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var msg = await _readerBuffer.ReceiveAsync(timeout);
                if (msg != null)
                {
                    yield return msg.Value;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
