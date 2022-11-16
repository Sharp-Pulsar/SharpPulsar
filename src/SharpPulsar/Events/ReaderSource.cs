using Akka.Actor;
using SharpPulsar.Admin.v2;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SharpPulsar.Events
{
    public class ReaderSource<T>
    {
        private readonly IActorRef _eventSource;
        private readonly HttpClient _httpclient;
        private readonly PulsarAdminRESTAPIClient _admin;
        public ReaderSource(string brokerWebServiceUrl, IActorRef sourceActor)
        {
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
        /// Reads existing events and future events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public async IAsyncEnumerable<T> Events(TimeSpan timeout, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                AskResponse response = null;
                try
                {
                    response = await _eventSource.Ask<AskResponse>(EventSource.Messages.Receive.Instance, timeout);
                }
                catch { }
                if (response != null && response.Data != null)
                {
                    yield return response.ConvertTo<IMessage<T>>().Value;
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
                if (_eventSource.IsNobody())
                    throw new Exception("Source Reader Terminated");
                AskResponse response = null;
                try
                {
                    response = await _eventSource.Ask<AskResponse>(EventSource.Messages.Receive.Instance, timeout);
                }
                catch { }
                if (response != null && response.Data != null)
                {
                    yield return response.ConvertTo<IMessage<T>>().Value;
                }
                else
                    break;
            }
        }
    }
}
