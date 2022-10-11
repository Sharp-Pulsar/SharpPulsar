using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Akka.Actor;
using DotNetty.Common.Utilities;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol;
using static SharpPulsar.PatternMultiTopicsConsumer<T>;
using Akka.Event;

namespace SharpPulsar
{

    internal class TopicListWatcherActor : ReceiveActor
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
        private readonly Regex _topicsPattern;
        private readonly long _watcherId;
        private long _createWatcherDeadline = 0;
        private readonly NamespaceName _namespace;
        private readonly ITopicsChangedListener _topicsChangeListener;
        private string _topicsHash;
        private readonly ILoggingAdapter _log;
        protected internal HandlerState _state;
        private TaskCompletionSource<IActorRef> _watcherFuture;
        public TopicListWatcherActor(ITopicsChangedListener topicsChangeListener, IActorRef client, ClientConfigurationData conf, Regex topicsPattern, long watcherId, NamespaceName @namespace, string topicsHash, HandlerState state, TaskCompletionSource<IActorRef> watcherFuture)
        {
            _lookupDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + conf.LookupTimeout.TotalMilliseconds);
            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(conf, state, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(conf.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(conf.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));
            _state = state;
            _topicsChangeListener = topicsChangeListener;
            _name = "Watcher(" + topicsPattern + ")";
            _topicsPattern = topicsPattern;
            _watcherId = watcherId;
            _namespace = @namespace;
            _topicsHash = topicsHash;
            _log = Context.GetLogger();
            _watcherFuture = watcherFuture;
            GrabCnx();
        }
        private void GrabCnx()
        {
            _connectionHandler.Tell(new GrabCnx($"Create connection from topicListWatcher: {_name}"));
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

        public virtual void ConnectionOpened(IActorRef cnx)
        {
            _previousExceptions.Clear();

            if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
            {
                _state.ConnectionState = HandlerState.State.Closed;
                DeregisterFromClientCnx();
                return;
            }

            _log.Info("[{}][{}] Creating topic list watcher on cnx {}, watcherId {}", Topic, HandlerName, Cnx.Ctx().channel(), watcherId);

            long RequestId = ClientConflict.NewRequestId();

            createWatcherDeadlineUpdater.compareAndSet(this, 0L, DateTimeHelper.CurrentUnixTimeMillis() + ClientConflict.Configuration.getOperationTimeoutMs());

            // synchronized this, because redeliverUnAckMessage eliminate the epoch inconsistency between them
            lock (this)
            {
                ClientCnx = Cnx;
                BaseCommand WatchRequest = Commands.newWatchTopicList(RequestId, watcherId, @namespace.ToString(), topicsPattern.pattern(), topicsHash);

                Cnx.NewWatchTopicList(WatchRequest, RequestId).thenAccept(response =>
                {
                    lock (TopicListWatcher.this)

                {
                    if (!ChangeToReadyState())
                    {
                        State = State.Closed;
                        DeregisterFromClientCnx();
                        Cnx.Channel().close();
                        return;
                    }
                }
                this.connectionHandler.ResetBackoff();
                watcherFuture.complete(this);
            }).exceptionally((e) =>
            {
                DeregisterFromClientCnx();
                if (State == State.Closing || State == State.Closed)
                {
                    Cnx.Channel().close();
                    return null;
                }
                log.warn("[{}][{}] Failed to subscribe to topic on {}", Topic, HandlerName, Cnx.Channel().remoteAddress());
                if (e.getCause() is PulsarClientException && PulsarClientException.isRetriableError(e.getCause()) && DateTimeHelper.CurrentUnixTimeMillis() < createWatcherDeadlineUpdater.get(TopicListWatcher.this))
                {
                    ReconnectLater(e.getCause());
                }
                else if (!watcherFuture.isDone())
                {
                    State = State.Failed;
                    watcherFuture.completeExceptionally(PulsarClientException.wrap(e, string.Format("Failed to create topic list watcher {0}" + "when connecting to the broker", HandlerName)));
                }
                else
                {
                    ReconnectLater(e.getCause());
                }
                return null;
            });
        }
        private string HandlerName
        {
            get
            {
                return _name;
            }
        }

        public virtual bool Connected
        {
            get
            {
                return ClientCnx != null && (_state.ConnectionState == HandlerState.State.Ready);
            }
        }

        public virtual ClientCnx ClientCnx
        {
            get
            {
                return this.connectionHandler.Cnx();
            }
            set
            {
                if (value != null)
                {
                    this.connectionHandler.ClientCnx = value;
                    value.RegisterTopicListWatcher(watcherId, this);
                }
                value PreviousClientCnx = clientCnxUsedForWatcherRegistration.getAndSet(value);
                if (PreviousClientCnx != null && PreviousClientCnx != value)
                {
                    PreviousClientCnx.RemoveTopicListWatcher(watcherId);
                }
            }
        }

        public virtual CompletableFuture<Void> CloseAsync()
        {

            CompletableFuture<Void> CloseFuture = new CompletableFuture<Void>();

            if (State == State.Closing || State == State.Closed)
            {
                CloseFuture.complete(null);
                return CloseFuture;
            }

            if (!Connected)
            {
                log.info("[{}] [{}] Closed watcher (not connected)", Topic, HandlerName);
                State = State.Closed;
                DeregisterFromClientCnx();
                CloseFuture.complete(null);
                return CloseFuture;
            }

            State = State.Closing;


            long RequestId = ClientConflict.NewRequestId();

            ClientCnx Cnx = Cnx();
            if (null == Cnx)
            {
                CleanupAtClose(CloseFuture, null);
            }
            else
            {
                BaseCommand Cmd = Commands.newWatchTopicListClose(watcherId, RequestId);
                Cnx.NewWatchTopicListClose(Cmd, RequestId).handle((v, exception) =>
                {
                    ChannelHandlerContext Ctx = Cnx.Ctx();
                    bool IgnoreException = Ctx == null || !Ctx.channel().isActive();
                    if (IgnoreException && exception != null)
                    {
                        log.debug("Exception ignored in closing watcher", exception);
                    }
                    CleanupAtClose(CloseFuture, IgnoreException ? null : exception);
                    return null;
                });
            }

            return CloseFuture;
        }

        // wrapper for connection methods
        private IActorRef Cnx()
        {
            return _connectionHandler.Cnx();
        }

        public virtual void ConnectionClosed(ClientCnx ClientCnx)
        {
            this.connectionHandler.ConnectionClosed(ClientCnx);
        }


        internal virtual void DeregisterFromClientCnx()
        {
            ClientCnx = null;
        }

        internal virtual void ReconnectLater(Exception Exception)
        {
            this.connectionHandler.ReconnectLater(Exception);
        }


        private void CleanupAtClose(CompletableFuture<Void> CloseFuture, Exception Exception)
        {
            log.info("[{}] Closed topic list watcher", HandlerName);
            State = State.Closed;
            DeregisterFromClientCnx();
            if (Exception != null)
            {
                CloseFuture.completeExceptionally(Exception);
            }
            else
            {
                CloseFuture.complete(null);
            }
        }

        public virtual void HandleCommandWatchTopicUpdate(CommandWatchTopicUpdate Update)
        {
            IList<string> Deleted = Update.getDeletedTopicsList();
            if (Deleted.Count > 0)
            {
                topicsChangeListener.OnTopicsRemoved(Deleted);
            }
            IList<string> Added = Update.getNewTopicsList();
            if (Added.Count > 0)
            {
                topicsChangeListener.OnTopicsAdded(Added);
            }
        }
    }
}
