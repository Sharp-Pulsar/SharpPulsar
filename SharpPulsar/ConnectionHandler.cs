using Akka.Actor;
using Akka.Event;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using System;
using System.Text;
using BAMCIS.Util.Concurrent;
using State = SharpPulsar.HandlerState.State;

namespace SharpPulsar
{	
	public sealed class GetConnection
	{
		public string Topic { get; }
        public GetConnection(string topic)
        {
			Topic = topic;
        }
    }
	public class ConnectionHandler
	{
		internal interface IConnection
		{
			void ConnectionFailed(PulsarClientException exception);
			void ConnectionOpened(ClientCnx cnx);
		}
		private ClientCnx _clientCnx = null;

		private readonly HandlerState _state;
		private readonly Backoff _backoff;
		private readonly long _epoch = 0L;
		protected long LastConnectionClosedTimestamp = 0L;
		private readonly ILoggingAdapter _log;

		internal IConnection Connection;

		internal ConnectionHandler(HandlerState state, Backoff backoff, IConnection connection, ILoggingAdapter logger)
		{
			_state = state;
			Connection = connection;
			_backoff = backoff;
			_log = logger;
		}

		protected internal virtual void GrabCnx()
		{
			if (_clientCnx != null)
			{
				_log.Warning("[{}] [{}] Client cnx already set, ignoring reconnection request", _state.Topic, _state.HandlerName);
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				_log.Info("[{}] [{}] Ignoring reconnection request (state: {})", _state.Topic, _state.HandlerName, _state);
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
				_log.Warning("[{}] [{}] Exception thrown while getting connection: ", _state.Topic, _state.HandlerName, t);
				ReconnectLater(t);
			}
		}

		private void HandleConnectionError(Exception exception)
		{
			_log.Warning("[{}] [{}] Error connecting to broker: {}", _state.Topic, _state.HandlerName, exception.Message);
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
				_log.Info("[{}] [{}] Ignoring reconnection request (state: {})", _state.Topic, _state.HandlerName, _state.ConnectionState);
				return;
			}
			long delayMs = _backoff.Next();
			_log.Warning("[{}] [{}] Could not get connection to broker: {} -- Will try again in {} s", _state.Topic, _state.HandlerName, exception.Message, delayMs / 1000.0);
			_state.ConnectionState = State.Connecting;
			State.ClientConflict.Timer().newTimeout(timeout =>
			{
				_log.info("[{}] [{}] Reconnecting after connection was closed", State.Topic, State.HandlerName);
				++EpochConflict;
				GrabCnx();
			}, delayMs, TimeUnit.MILLISECONDS);
		}

		public virtual void ConnectionClosed(ClientCnx cnx)
		{
			LastConnectionClosedTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
			_state.ClientConflict.CnxPool.releaseConnection(cnx);
			if (_clientCnxUpdater.compareAndSet(this, cnx, null))
			{
				if (!ValidStateForReconnection)
				{
					_log.info("[{}] [{}] Ignoring reconnection request (state: {})", _state.Topic, _state.HandlerName, _state.ConnectionState);
					return;
				}
				long delayMs = _backoff.Next();
				State.State = State.Connecting;
				_log.info("[{}] [{}] Closed connection {} -- Will try again in {} s", _state.Topic, _state.HandlerName, cnx.Channel(), delayMs / 1000.0);
				State.ClientConflict.Timer().newTimeout(timeout =>
				{
					_log.info("[{}] [{}] Reconnecting after timeout", State.Topic, State.HandlerName);
					++EpochConflict;
					GrabCnx();
				}, delayMs, TimeUnit.MILLISECONDS);
			}
		}

		protected internal virtual void ResetBackoff()
		{
			_backoff.Reset();
		}

		public virtual ClientCnx Cnx()
		{
			return _clientCnx;
		}

		protected  ClientCnx ClientCnx
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
