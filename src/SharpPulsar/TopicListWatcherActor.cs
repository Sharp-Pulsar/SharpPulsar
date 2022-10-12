using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol;
using Akka.Event;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Transaction;

namespace SharpPulsar
{

    internal class TopicListWatcherActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _connectionHandler;
        private readonly IActorRef _self;
        private IActorRef _sender;

        private readonly IScheduler _scheduler;
        private IActorRef _replyTo;
        private long _requestId = -1;
        private IActorRef _cnx;
        private readonly TimeSpan _lookupDeadline;
        private readonly Collection<Exception> _previousExceptions = new Collection<Exception>();
        private readonly string _name;
        private readonly string _topicsPattern;
        private readonly long _watcherId;
        private long _createWatcherDeadline = 0;
        private readonly NamespaceName _namespace;
        private string _topicsHash;
        private readonly ILoggingAdapter _log;
        protected internal HandlerState _state;
        private TaskCompletionSource<IActorRef> _watcherFuture;
        private ClientConfigurationData _conf;
        private readonly IActorRef _generator;
        private IActorRef _clientCnxUsedForWatcherRegistration;
        public TopicListWatcherActor(IActorRef client, IActorRef idGenerator, ClientConfigurationData conf, string topicsPattern, long watcherId, NamespaceName @namespace, string topicsHash, HandlerState state)
        {
            _lookupDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + conf.LookupTimeout.TotalMilliseconds);
            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(conf, state, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(conf.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(conf.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));
            _state = state;
            _name = "Watcher(" + topicsPattern + ")";
            _topicsPattern = topicsPattern;
            _watcherId = watcherId;
            _namespace = @namespace;
            _topicsHash = topicsHash;
            _log = Context.GetLogger();
           // _watcherFuture = watcherFuture;
            _conf = conf;
            _generator = idGenerator; 
        }
        public static Props Prop(IActorRef client, IActorRef idGenerator, ClientConfigurationData conf, string topicsPattern, long watcherId, NamespaceName @namespace, string topicsHash, HandlerState state)
        {
            return Props.Create(() => new TopicListWatcherActor(client, idGenerator, conf, topicsPattern, watcherId, @namespace, topicsHash, state));
        }
        private void GrabCnx()
        {
            Receive<Grab>(_ =>
            {
                _replyTo = Sender;
                _connectionHandler.Tell(new GrabCnx($"Create connection from topicListWatcher: {_name}"));
                Become(Connection);
            });
            ReceiveAny(s=> Stash.Stash());
        }
        private void Connection()
        {
            ReceiveAsync<AskResponse>(async ask =>
            {
                if (ask.Failed)
                {
                    ConnectionFailed(ask.Exception);
                    _replyTo.Tell(ask);
                }
                else
                {
                    var con =  await ConnectionOpened(ask.ConvertTo<ConnectionOpened>()).ConfigureAwait(false);
                    if(con != null)
                    {
                        _replyTo.Tell(con);
                        Become(Handle);
                    }
                        
                }
                   
            });
            ReceiveAny(s => Stash.Stash());
        }
        private void Handle()
        {
            Receive<Grab>(_ =>
            {
                _replyTo = Sender;
                _connectionHandler.Tell(new GrabCnx($"Create connection from topicListWatcher: {_name}"));
                Become(Connection);
            });
            Receive<Close>(_ =>
            {
               Sender.Tell(Close());
            });
            Receive<HandleWatchTopicUpdate>(update =>
            {
                HandleCommandWatchTopicUpdate(update.Update, Sender);
            });
            Receive<ConnectionClosed>(ctx =>
            {
                ConnectionClosed(ctx.ClientCnx);
            });
            Stash?.UnstashAll();

        }
        protected override void PreStart()
        {
            base.PreStart();
            Become(GrabCnx);
        }
        private void ConnectionFailed(PulsarClientException exception)
        {
            var nonRetriableError = !PulsarClientException.IsRetriableError(exception);
            if (nonRetriableError)
            {
                exception.SetPreviousExceptions(_previousExceptions);
                if (true)
                {
                    _watcherFuture.SetException(exception);
                    _state.ConnectionState = HandlerState.State.Failed;
                    _log.Info($"[Topic] Watcher creation failed for {_name} with non-retriable error {exception}");
                    DeregisterFromClientCnx();
                }
            }
            else
            {
                _previousExceptions.Add(exception);
            }
        }

        private async ValueTask<AskResponse> ConnectionOpened(ConnectionOpened c)
        {
            ClientCnx = c.ClientCnx;
            _previousExceptions.Clear();

            if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
            {
                _state.ConnectionState = HandlerState.State.Closed;
                DeregisterFromClientCnx();
                return new AskResponse(_state.ConnectionState);
            }

            _log.Info($"[Topic][{HandlerName}] Creating topic list watcher on cnx, watcherId {_watcherId}");
            var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).ConfigureAwait(false);
            var requestId = id.Id;
            _createWatcherDeadline =  DateTimeHelper.CurrentUnixTimeMillis() + (long)_conf.OperationTimeout.TotalMilliseconds;

            // synchronized this, because redeliverUnAckMessage eliminate the epoch inconsistency between them
            var watchRequest = Commands.NewWatchTopicList(requestId, _watcherId, _namespace.ToString(), _topicsPattern, _topicsHash);
            try
            {
                var response = await _cnx.Ask<CommandWatchTopicListSuccessResponse>(new Payload(watchRequest, requestId, "NewWatchTopicList"), _conf.OperationTimeout).ConfigureAwait(false);
                if (!_state.ChangeToReadyState())
                {
                    _state.ConnectionState = HandlerState.State.Closed;
                    DeregisterFromClientCnx();
                    _cnx.Tell(Messages.Requests.Close.Instance);
                    return new AskResponse(_state.ConnectionState);
                }
                ResetBackoff();
                //watcherFuture.complete(this);
                return new AskResponse(c);
            }
            catch (Exception e) 
            {
                DeregisterFromClientCnx();
                if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
                {
                    _cnx.Tell(Messages.Requests.Close.Instance);
                }
                _log.Warning($"[Topic][{HandlerName}] Failed to subscribe to topic on 'remoteAddress'");
                if (e.InnerException is PulsarClientException && PulsarClientException.IsRetriableError(e.InnerException) && DateTimeHelper.CurrentUnixTimeMillis() < _createWatcherDeadline)
                {
                    ReconnectLater(e.InnerException);
                    return null;
                }
                else
                {
                    _state.ConnectionState = HandlerState.State.Failed;
                    return new AskResponse(PulsarClientException.Wrap(e, $"Failed to create topic list watcher {HandlerName} when connecting to the broker"));
                }
            }
            
        }
        private string HandlerName
        {
            get
            {
                return _name;
            }
        }

        private void ResetBackoff()
        {
            _connectionHandler.Tell(Messages.Requests.ResetBackoff.Instance);
        }
        private bool Connected
        {
            get
            {
                return ClientCnx != null && (_state.ConnectionState == HandlerState.State.Ready);
            }
        }

        public virtual IActorRef ClientCnx
        {
            get
            {
                return _cnx;
            }
            set
            {
                if (value != null)
                {
                    _cnx = value;
                    _cnx.Tell(new RegisterTopicListWatcher(_watcherId, Self));
                }
                var previousClientCnx = _clientCnxUsedForWatcherRegistration = value;
                if (previousClientCnx != null && previousClientCnx != value)
                {
                    previousClientCnx.Tell(new RemoveTopicListWatcher(_watcherId));
                }
            }
        }

        public IStash Stash { get; set; }

        private async ValueTask<AskResponse> Close()
        {

            if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
            {
                return new AskResponse();
            }

            if (!Connected)
            {
                _log.Info($"[Topic] [{HandlerName}] Closed watcher (not connected)");
                _state.ConnectionState = HandlerState.State.Closed;
                DeregisterFromClientCnx();
                return new AskResponse(null);
            }

            _state.ConnectionState = HandlerState.State.Closing;

            var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).ConfigureAwait(false);
            var requestId = id.Id;
            CommandSuccess response = null;
            if (null == _cnx)
            {
                CleanupAtClose(null);
            }
            else
            {
                try
                {
                    var cmd = Commands.NewWatchTopicListClose(_watcherId, requestId);
                    response = await _cnx.Ask<CommandSuccess>(new Payload(cmd, requestId, "NewWatchTopicListClose"), _conf.OperationTimeout).ConfigureAwait(false);

                }
                catch(Exception ex)
                {
                    _log.Debug($"Exception ignored in closing watcher {ex}");
                    CleanupAtClose(ex);
                }
                
            }

            return new AskResponse(response);
        }

        // wrapper for connection methods
        private IActorRef Cnx()
        {
            return _cnx;
        }

       private void ConnectionClosed(IActorRef clientCnx)
        {
            _connectionHandler.Tell(new ConnectionClosed(clientCnx));
        }


        private void DeregisterFromClientCnx()
        {
            ClientCnx = null;
        }

        private void ReconnectLater(Exception exception)
        {
            _connectionHandler.Tell(new ReconnectLater(exception));
        }


        private Exception CleanupAtClose(Exception exception)
        {
            _log.Info($"[{HandlerName}] Closed topic list watcher");
            _state.ConnectionState = HandlerState.State.Closed; 
            DeregisterFromClientCnx();
            if (exception != null)
            {
                return exception;
            }
            else
            {
                return null;
            }
        }

        private void HandleCommandWatchTopicUpdate(CommandWatchTopicUpdate update, IActorRef sender)
        {
            IList<string> deleted = update.DeletedTopics;
            if (deleted.Count > 0)
            {
               sender.Tell(new TopicsRemoved(deleted));
            }
            IList<string> added = update.NewTopics;
            if (added.Count > 0)
            {
                sender.Tell(new TopicsAdded(added));
            }
        }
    }
}
