using Akka.Actor;
using Akka.Event;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using System;
using System.Text;
using BAMCIS.Util.Concurrent;
using State = SharpPulsar.HandlerState.State;
using SharpPulsar.Messages;

namespace SharpPulsar
{	
	public class ConnectionHandler:ReceiveActor
	{
		public interface IConnection
		{
			void ConnectionFailed(PulsarClientException exception);
			void ConnectionOpened(ClientCnx cnx);
		}
		private sealed class GetCnx
        {
			public string Message { get; }
            public GetCnx(string message)
            {
				Message = message;
            }
        }
		private ClientCnx _clientCnx = null;

		private readonly HandlerState _state;
		private readonly Backoff _backoff;
		private long _epoch = 0L;
		protected long LastConnectionClosedTimestamp = 0L;
		private readonly ILoggingAdapter _log;
		internal IConnection Connection;
		private ICancelable _cancelable;
		private readonly IActorContext _actorContext;

		internal ConnectionHandler(HandlerState state, Backoff backoff, IConnection connection)
		{
			_state = state;
			Connection = connection;
			_backoff = backoff;
			_log = Context.System.Log;
			_actorContext = Context;
			Receive<GetCnx>(g =>
			{
				_log.Info(g.Message);
				++_epoch;
				GrabCnx();
			});
		}

		private void GrabCnx()
		{
			if (_clientCnx != null)
			{
				_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Client cnx already set, ignoring reconnection request");
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				return;
			}

			try
			{
				_state.Client.Ask<ClientCnx>(new GetConnection(_state.Topic)).ContinueWith(task => 
				{
					if (task.IsFaulted)
						HandleConnectionError(task.Exception);
					else
						Connection.ConnectionOpened(task.Result);
				});
			}
			catch (Exception t)
			{
				_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Exception thrown while getting connection: {t}");
				ReconnectLater(t);
			}
		}

		private void HandleConnectionError(Exception exception)
		{
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Error connecting to broker: {exception.Message}");
			if (exception is PulsarClientException)
			{
				Connection.ConnectionFailed((PulsarClientException)exception);
			}
			else if (exception.InnerException is PulsarClientException)
			{
				Connection.ConnectionFailed((PulsarClientException)exception.InnerException);
			}
			else
			{
				Connection.ConnectionFailed(new PulsarClientException(exception));
			}

			var state = _state.ConnectionState;
			if (state == State.Uninitialized || state == State.Connecting || state == State.Ready)
			{
				ReconnectLater(exception);
			}

			return;
		}

		protected internal virtual void ReconnectLater(Exception exception)
		{
			_clientCnx = null;
			if (!ValidStateForReconnection)
			{
				_log.Info($"[{_state.Topic}] [{_state.HandlerName}] Ignoring reconnection request (state: {_state.ConnectionState})");
				return;
			}
			long delayMs = _backoff.Next();
			_log.Warning($"[{_state.Topic}] [{_state.HandlerName}] Could not get connection to broker: {exception.Message} -- Will try again in {(delayMs/1000.0)} s");
			_state.ConnectionState = State.Connecting;
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(delayMs)), Self, new GetCnx($"[{_state.Topic}] [{_state.HandlerName}] Reconnecting after connection was closed"), Nobody.Instance);
		}

		public virtual void ConnectionClosed(ClientCnx cnx)
		{
			LastConnectionClosedTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
			_state.Client.Ask(new ReleaseConnectionPool(cnx));
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
			_cancelable = _actorContext.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(delayMs)), Self, new GetCnx($"[{ _state.Topic}] [{_state.HandlerName}] Reconnecting after timeout"), Nobody.Instance);
			
		}

		protected internal virtual void ResetBackoff()
		{
			_backoff.Reset();
		}
		public static Props Prop(HandlerState state, Backoff backoff, IConnection connection)
        {
			return Props.Create(() => new ConnectionHandler(state, backoff, connection));
        }
        protected override void PostStop()
        {
			_cancelable?.Cancel();
            base.PostStop();
        }
        public virtual ClientCnx Cnx()
		{
			return _clientCnx;
		}

		protected internal ClientCnx ClientCnx
		{
			set
			{
				_clientCnx = value;
			}
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

		public long Epoch
		{
			get
			{
				return _epoch;
			}
		}

	}

}
