using Akka.Actor;
using SharpPulsar.Exceptions;
using System;
using System.Threading.Tasks;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Configuration;
using SharpPulsar.Common.Naming;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Client;
using Akka.Util;
using SharpPulsar.ServiceName;

namespace SharpPulsar.Client
{
    public class ConnectionHandler : ReceiveActor, IWithUnboundedStash, IWithTimers
    {
        private IActorRef _clientCnx = null;
        private AtomicBoolean _duringConnect = new AtomicBoolean(false);
        protected int _randomKeyForSelectConnection;

        private bool _useProxy;
        //private readonly IActorRef _state;
        private readonly ClientConfigurationData _conf;
        private readonly Backoff _backoff;
        private long _epoch = 0L;
        protected long LastConnectionClosedTimestamp = 0L;
        private readonly ILoggingAdapter _log;
        private readonly IActorRef _connection;
        private ICancelable _cancelable;
        private readonly IActorContext _actorContext;
        private IActorRef _sender;
        private IActorRef _state;
        private HandlerAll _Allstate;
        
        public ConnectionHandler(ClientConfigurationData conf, IActorRef state, Backoff backoff, IActorRef connection)
        {
            //_state = state;
            _connection = connection;
            _backoff = backoff;
            _log = Context.GetLogger();
            _actorContext = Context;
            _conf = conf;
            _state = state;// state.Ask<HandlerAll>(GetAll.Instance).GetAwaiter().GetResult();
            PublicListening();
        }
        private void PublicListening()
        {
            ReceiveAsync<GrabCnx>(async g =>
            {
                _log.Info(g.Message);
                ++_epoch;
                Become(PrivateListening);
                await GrabCnx(Sender);
            });
            
            Receive<ReconnectLater>(g =>
            {
                Become(PrivateListening);
                ReconnectLater(g.Exception, Sender);
            });
            Receive<GetEpoch>(g =>
            {
                Sender.Tell(new GetEpochResponse(_epoch));
            });

            Receive<SwitchClientCnx>(c =>
            {
                _clientCnx = c.ClientCnx;
                ++_epoch;
                Sender.Tell(new GetEpochResponse(_epoch));
            });
            Receive<ConnectionOpened>(m =>
            {
                _duringConnect.GetAndSet(false);
                _connection.Tell(m);
            });
            Receive<ConnectionFailed>(m =>
            {
                Become(PrivateListening);
                HandleConnectionError(m.Exception);
            });
            Receive<ResetBackoff>(_ =>
            {
                ResetBackoff();
            });
            Receive<LastConnectionClosedTimestamp>(_ =>
            {
                Sender.Tell(new LastConnectionClosedTimestampResponse(LastConnectionClosedTimestamp));
            });
            Receive<GetCnx>(_ =>
            {
                Sender.Tell(new AskResponse(_clientCnx));
            });
            Receive<SetCnx>(s =>
            {
                _clientCnx = s.ClientCnx;
            });
            ReceiveAsync<ConnectionClosed>(async c =>
            {
                Become(PrivateListening);
                var children = Context.GetChildren();
                foreach (var child in children)
                    _ = child.GracefulStop(TimeSpan.FromMilliseconds(100));
                ConnectionClosed(c.ClientCnx);
            });
        }
        private void PrivateListening()
        {
            ReceiveAsync<GrabRec>(async g =>
            {
                _log.Info(g.GrabCnx.Message);
                ++_epoch;
                await GrabCnx(g.ReplyTo);
            });
            ReceiveAny(s => Stash.Stash());
        }
        private void UnstashAll()
        {
            Stash?.UnstashAll();
            Become(PublicListening);
        }
        private async ValueTask GrabCnx(IActorRef sender)
        {
            var state = await _state.Ask<HandlerAll>(GetAll.Instance);
            if (!_duringConnect.CompareAndSet(false, true))
            {
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Skip grabbing the connection since there is a pending connection");
                sender.Tell(new ConnectionAlreadySet(_clientCnx));
                UnstashAll();
                return;
            }

            if (_clientCnx != null)
            {
                _log.Warning($"[{state.Topic}] [{state.HandlerName}] Client cnx already set, ignoring reconnection request");
                sender.Tell(new ConnectionAlreadySet(_clientCnx));
                UnstashAll();
                return;
            }

            if (!ValidStateForReconnection)
            {
                // Ignore connection closed when we are shutting down
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Ignoring reconnection request (state: {state.State})");
                sender.Tell(new AskResponse(PulsarClientException.Unwrap(new Exception("Invalid State For Reconnection"))));
                UnstashAll();
                return; 
            }
            await LookupConnection(state, sender);
        }
        private async ValueTask LookupConnection(HandlerAll state, IActorRef sender)
        {
            if (state.RedirectedClusterURI != null)
            {
                if (string.IsNullOrWhiteSpace(state.Topic))
                {
                    var c = await state.ConnectionPool
                        .Ask<AskResponse>(new GetConnection(state.RedirectedClusterURI.ToDnsEndPoint(), state.RedirectedClusterURI.ToDnsEndPoint()));
                    if (c.Failed)
                    {
                        _log.Warning($"[{state.Topic}] [{state.HandlerName}] Exception thrown while getting connection: {c.Exception}");
                        ReconnectLater(c.Exception, sender);
                        return;
                    }
                    sender.Tell(c);
                    UnstashAll();
                    return;
                }
                await TopicLookup(state, sender);
                return;
            }
            else if (string.IsNullOrWhiteSpace(state.Topic))
            {
                var sUrl = await state.Lookup.Ask<Uri>(GetResolvedHost.Instance);
                var connect1 = await state.ConnectionPool.Ask<AskResponse>(new GetConnection(sUrl.ToDnsEndPoint()));
                if (connect1.Failed)
                {
                    _log.Warning($"[{state.Topic}] [{state.HandlerName}] Exception thrown while getting connection: {connect1.Exception}");
                    ReconnectLater(connect1.Exception, sender);
                    return;
                }
                sender.Tell(connect1);
                UnstashAll();
                return;
            }

            await TopicLookup(state, sender);
        }
        private async ValueTask TopicLookup(HandlerAll state, IActorRef sender)
        {
            var topicName = TopicName.Get(state.Topic);
            var askResponse = await state.Lookup.Ask<AskResponse>(new GetBroker(topicName));
            if (askResponse.Failed)
            {
                sender.Tell(askResponse);
                UnstashAll();
                return;
            }

            var broker = askResponse.ConvertTo<GetBrokerResponse>();
            var connect = await state.ConnectionPool.Ask<AskResponse>(new GetConnection(broker.LogicalAddress, broker.PhysicalAddress));
            if (connect.Failed)
            {
                _log.Warning($"[{state.Topic}] [{state.HandlerName}] Exception thrown while getting connection: {connect.Exception}");
                ReconnectLater(connect.Exception, sender);
                return;
            }
            sender.Tell(connect);
            UnstashAll();
        }
        private void HandleConnectionError(Exception exception)
        {
            var state = _state.Ask<HandlerAll>(GetAll.Instance).GetAwaiter().GetResult();
            _log.Warning($"[{state.Topic}] [{state.HandlerName}] Error connecting to broker: {exception.Message}");
            if (exception is PulsarClientException clientException)
            {
                _connection.Tell(new ConnectionFailed(clientException));
            }
            else if (exception.InnerException is PulsarClientException innerException)
            {
                _connection.Tell(new ConnectionFailed(innerException));
            }
            else
            {
                _connection.Tell(new Status.Failure(exception));
            }

            ReconnectLater(exception, Sender);
        }

        private void ReconnectLater(Exception exception, IActorRef sender)
        {
            _duringConnect.GetAndSet(false);
            var state = _state.Ask<HandlerAll>(GetAll.Instance).GetAwaiter().GetResult();
            var reply = sender ?? _connection;
            var children = Context.GetChildren();
            foreach (var child in children)
                child.GracefulStop(TimeSpan.FromMilliseconds(100));

            _clientCnx = null;
            if (!ValidStateForReconnection)
            {
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Ignoring reconnection request (state: {state.State})");
                UnstashAll();
                return;
            }
            var delayMs = _backoff.Next();
            _log.Warning($"[{state.Topic}] [{state.HandlerName}] Could not get connection to broker: {exception.Message} -- Will try again in {delayMs / 1000.0} s");
            var change = _state.Ask<bool>(ChangeToConnecting.Instance).GetAwaiter().GetResult();
            if (change)
            {
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Reconnecting after connection was closed");
                Timers.StartSingleTimer(GrabRec.Instance, 
                    new GrabRec(new GrabCnx($"[{state.Topic}] [{state.HandlerName}] Reconnecting after connection was closed"), reply), TimeSpan.FromMilliseconds(delayMs));
                return;
            }
            else
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Ignoring reconnection request (state: {state.State})");
            //_state.State = State.Connecting;
            //_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(delayMs), Self, new GrabCnx($"[{state.Topic}] [{state.HandlerName}] Reconnecting after connection was closed"), reply);
            UnstashAll();
        }

        private void ConnectionClosed(IActorRef cnx)
        {
            var state = _state.Ask<HandlerAll>(GetAll.Instance).GetAwaiter().GetResult();
            LastConnectionClosedTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
            _clientCnx = null;
            if (!ValidStateForReconnection)
            {
                _log.Info($"[{state.Topic}] [{state.HandlerName}] Ignoring reconnection request (state: {state.State})");
                UnstashAll();
                return;
            }
            var delayMs = _backoff.Next();
            state.StateActor.Tell(new SetState(State.Connecting));
            //_state.State = State.Connecting;
            //_log.Info("[{}] [{}] Closed connection -- Will try again in {} s", _state.Topic, _state.HandlerName, cnx.Channel()delayMs / 1000.0);
            _log.Info($"[{state.Topic}] [{state.HandlerName}] Closed connection -- Will try again in {delayMs / 1000.0} s");
            Timers.StartSingleTimer(GrabRec.Instance,
                    new GrabRec(new GrabCnx($"[{state.Topic}] [{state.HandlerName}] Reconnecting after timeout"), _connection), TimeSpan.FromMilliseconds(delayMs));
            //_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(delayMs), Self, new GrabCnx($"[{_state.Topic}] [{_state.HandlerName}] Reconnecting after timeout"), _connection);

        }

        private void ResetBackoff()
        {
            _backoff.Reset();
        }
        public static Props Prop(ClientConfigurationData conf, IActorRef state, Backoff backoff, IActorRef connection)
        {
            return Props.Create(() => new ConnectionHandler(conf, state, backoff, connection));
        }
        protected override void PostStop()
        {
            _cancelable?.Cancel();
            base.PostStop();
        }

        private bool ValidStateForReconnection
        {
            get
            {
                var state = _state.Ask<HandlerAll>(GetAll.Instance).GetAwaiter().GetResult().State;
                switch (state)
                {
                    case State.Uninitialized:
                    case State.Connecting:
                    case State.Ready:
                        // Ok
                        return true;

                    case State.Closing:
                    case State.Closed:
                    case State.Failed:
                    case State.ProducerFenced:
                    case State.Terminated:
                        return false;
                }
                return false;
            }
        }

        public ITimerScheduler Timers { get; set; }
        public IStash Stash { get; set; }

        internal record struct GrabRec(GrabCnx GrabCnx, IActorRef ReplyTo)
        {
            internal static GrabRec Instance { get; } = new GrabRec();  
        }
    }

}
