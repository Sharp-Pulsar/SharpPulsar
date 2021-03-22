using Akka.Actor;
using Akka.Event;
using SharpPulsar.Exceptions;
using System;
using BAMCIS.Util.Concurrent;
using State = SharpPulsar.HandlerState.State;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Extension;
using System.Net;
using SharpPulsar.Configuration;
using SharpPulsar.Common.Naming;
using System.Threading.Tasks;

namespace SharpPulsar
{
    public class ConnectionHandler:ReceiveActor
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

		public ConnectionHandler(ClientConfigurationData conf, HandlerState state, Backoff backoff, IActorRef connection)
		{
			_state = state;
			_connection = connection;
			_backoff = backoff;
			_log = Context.System.Log;
			_actorContext = Context;
			_conf = conf;
			Listening();
		}
		private void Listening()
        {

			Receive<GrabCnx>(g =>
			{
				_log.Info(g.Message);
				++_epoch;
				Become(GrabCnx);
			});
			Receive<ReconnectLater>(g =>
			{
				ReconnectLater(g.Exception);
			});
			Receive<GetEpoch>(g =>
			{
				Sender.Tell(_epoch);
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
				Sender.Tell(LastConnectionClosedTimestamp);
			});
			Receive<GetCnx>(_ =>
			{
				Sender.Tell(_clientCnx);
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
		}
		private void GrabCnx()
		{
			if (_clientCnx != null)
			{
				_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Client cnx already set, ignoring reconnection request");
				_connection.Tell(ConnectionAlreadySet.Instance);
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				_connection.Tell(new Failure { Exception = new Exception("Invalid State For Reconnection"), Timestamp = DateTime.UtcNow });
			}
			Receive<GetBrokerResponse>(broker =>
			{
				var connection = CreateConnection(broker.LogicalAddress, broker.PhysicalAddress);
				_connection.Tell(new ConnectionOpened(connection));
				Become(Listening);
			});
			Receive<GetConnectionResponse>(c =>
			{
				_connection.Tell(new ConnectionOpened(c.ClientCnx));
				Become(Listening);
			});
			Receive<Failure>(c =>
			{
				HandleConnectionError(c.Exception);
				Become(Listening);
			});
			if (_conf.UseDedicatedConnections)
			{
				_state.Lookup.Tell(new GetBroker(TopicName.Get(_state.Topic)));
			}
			else
			{
				_state.Lookup.Tell(new GetConnection(_state.Topic));
			}
		}
		private IActorRef CreateConnection(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress)
		{
			string targetBroker = string.Empty;

			if (!logicalAddress.Equals(physicalAddress))
				targetBroker = $"{logicalAddress.Host}:{logicalAddress.Port}";

			return Context.ActorOf(Props.Create(()=> new ClientCnx(_conf, physicalAddress, targetBroker)), $"{logicalAddress.Host}".ToAkkaNaming());
		}
		private void HandleConnectionError(Exception exception)
		{
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Error connecting to broker: {exception.Message}");
			if (exception is PulsarClientException)
			{
				_connection.Tell(new ConnectionFailed((PulsarClientException)exception));
			}
			else if (exception.InnerException is PulsarClientException)
			{
				_connection.Tell(new ConnectionFailed((PulsarClientException)exception.InnerException));
			}
			else
			{
				_connection.Tell(new Failure { Exception = exception, Timestamp = DateTime.UtcNow });
			}

			var state = _state.ConnectionState;
			if (state == State.Uninitialized || state == State.Connecting || state == State.Ready)
			{
				ReconnectLater(exception);
			}
		}

		private void ReconnectLater(Exception exception)
		{
			var children = Context.GetChildren();
			foreach (var child in children)
				child.GracefulStop(TimeSpan.FromMilliseconds(100));

			_clientCnx = null;
			if (!ValidStateForReconnection)
			{
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				return;
			}
			long delayMs = _backoff.Next();
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Could not get connection to broker: {exception.Message} -- Will try again in {(delayMs/1000.0)} s");
			_state.ConnectionState = State.Connecting;
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(delayMs)), Self, new GrabCnx($"[{_state.Topic}] [{_state.HandlerName}] Reconnecting after connection was closed"), Nobody.Instance);
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
			long delayMs = _backoff.Next();
			_state.ConnectionState = State.Connecting;
			//_log.Info("[{}] [{}] Closed connection -- Will try again in {} s", _state.Topic, _state.HandlerName, cnx.Channel()delayMs / 1000.0);
			_log.Info($"[{ _state.Topic}] [{_state.HandlerName}] Closed connection -- Will try again in {(delayMs / 1000.0)} s");
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(delayMs)), Self, new GrabCnx($"[{ _state.Topic}] [{_state.HandlerName}] Reconnecting after timeout"), Nobody.Instance);
			
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
				State state = _state.ConnectionState;
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
					case State.Terminated:
						return false;
				}
				return false;
			}
		}

	}

}
