using Akka.Actor;
using Akka.Event;
using SharpPulsar.Exceptions;
using System;
using System.Threading.Tasks;
using State = SharpPulsar.HandlerState.State;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Configuration;
using SharpPulsar.Common.Naming;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar
{
    public class ConnectionHandler:ReceiveActor, IWithUnboundedStash
	{
		private IActorRef _clientCnx = null;

		private readonly HandlerState _state;
		private readonly ClientConfigurationData _conf;
		private readonly Backoff _backoff;
		private long _epoch = 0L;
		protected long LastConnectionClosedTimestamp = 0L;
		private readonly ILoggingAdapter _log;
		private readonly IActorRef _connection;
		private ICancelable _cancelable;
		private readonly IActorContext _actorContext;
        private IActorRef _sender;

		public ConnectionHandler(ClientConfigurationData conf, HandlerState state, Backoff backoff, IActorRef connection)
		{
			_state = state;
			_connection = connection;
			_backoff = backoff;
			_log = Context.GetLogger();
			_actorContext = Context;
			_conf = conf;
			Listening();
		}
		private void Listening()
        {
            ReceiveAsync<GrabCnx>(async g =>
			{
				_log.Info(g.Message);
				++_epoch;
                _sender = Sender;
				await GrabCnx();
			});
			Receive<ReconnectLater>(g =>
			{
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
				_connection.Tell(m);
			});
			Receive<ConnectionFailed>(m =>
			{
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
				var children = Context.GetChildren();
				foreach (var child in children)
					await child.GracefulStop(TimeSpan.FromMilliseconds(100));
				ConnectionClosed(c.ClientCnx);
			});
			Stash?.UnstashAll();
		}
		private async ValueTask GrabCnx()
		{
			if (_clientCnx != null)
			{
				_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Client cnx already set, ignoring reconnection request");
				Sender.Tell(new AskResponse(ConnectionAlreadySet.Instance));
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				_sender.Tell(new AskResponse(PulsarClientException.Unwrap(new Exception("Invalid State For Reconnection"))));
			}
			await LookupConnection();
		}
		private async ValueTask LookupConnection()
        {
			var topicName = TopicName.Get(_state.Topic);
			var askResponse = await _state.Lookup.Ask<AskResponse>(new GetBroker(topicName));
            if (askResponse.Failed)
            {
                _sender.Tell(askResponse);
                return;
            }

            var broker = askResponse.ConvertTo<GetBrokerResponse>();
            var connect = await _state.ConnectionPool.Ask<AskResponse>(new GetConnection(broker.LogicalAddress, broker.PhysicalAddress));
            if (connect.Failed)
            {
                _log.Error(connect.Exception.ToString());
            }
            _sender.Tell(connect);
        }
		private void HandleConnectionError(Exception exception)
		{
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Error connecting to broker: {exception.Message}");
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

			var state = _state.ConnectionState;
			if (state == State.Uninitialized || state == State.Connecting || state == State.Ready)
			{
				ReconnectLater(exception);
			}
		}

		private void ReconnectLater(Exception exception, IActorRef sender = null)
        {
            var reply = sender ?? _connection;
			var children = Context.GetChildren();
			foreach (var child in children)
				child.GracefulStop(TimeSpan.FromMilliseconds(100));

			_clientCnx = null;
			if (!ValidStateForReconnection)
			{
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				return;
			}
			var delayMs = _backoff.Next();
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Could not get connection to broker: {exception.Message} -- Will try again in {(delayMs/1000.0)} s");
			_state.ConnectionState = State.Connecting;
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(delayMs), Self, new GrabCnx($"[{_state.Topic}] [{_state.HandlerName}] Reconnecting after connection was closed"), reply);
		}

		private void ConnectionClosed(IActorRef cnx)
		{
			LastConnectionClosedTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
			_clientCnx = null;
			if (!ValidStateForReconnection)
			{
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				return;
			}
			var delayMs = _backoff.Next();
			_state.ConnectionState = State.Connecting;
			//_log.Info("[{}] [{}] Closed connection -- Will try again in {} s", _state.Topic, _state.HandlerName, cnx.Channel()delayMs / 1000.0);
			_log.Info($"[{ _state.Topic}] [{_state.HandlerName}] Closed connection -- Will try again in {(delayMs / 1000.0)} s");
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(delayMs), Self, new GrabCnx($"[{ _state.Topic}] [{_state.HandlerName}] Reconnecting after timeout"), _connection);
			
		}

		private void ResetBackoff()
		{
			_backoff.Reset();
		}
		public static Props Prop(ClientConfigurationData conf, HandlerState state, Backoff backoff, IActorRef connection)
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
				var state = _state.ConnectionState;
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

        public IStash Stash { get; set; }
    }

}
