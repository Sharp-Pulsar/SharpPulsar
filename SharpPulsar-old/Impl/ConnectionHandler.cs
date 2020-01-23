using BAMCIS.Util.Concurrent;
using SharpPulsar.Exception;
using System;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Impl
{
	using State = HandlerState.State;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ConnectionHandler
	{
		private static readonly AtomicReferenceFieldUpdater<ConnectionHandler, ClientCnx> CLIENT_CNX_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(ConnectionHandler), typeof(ClientCnx), "clientCnx");

		private volatile ClientConnection clientCnx = null;

		protected internal readonly HandlerState state;
		protected internal readonly Backoff backoff;
		protected internal long epoch = 0L;

		public interface Connection
		{
			void ConnectionFailed(PulsarClientException exception);
			void ConnectionOpened(ClientConnection cnx);
		}

		protected internal Connection connection;

		protected internal ConnectionHandler(HandlerState state, Backoff backoff, Connection connection)
		{
			this.state = state;
			this.connection = connection;
			this.backoff = backoff;
			CLIENT_CNX_UPDATER.set(this, null);
		}

		protected internal virtual void GrabCnx()
		{
			if (CLIENT_CNX_UPDATER.get(this) != null)
			{
				log.warn("[{}] [{}] Client cnx already set, ignoring reconnection request", state.topic, state.HandlerName);
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.GetState());
				return;
			}

			try
			{
				state.client.getConnection(state.topic).thenAccept(cnx => connection.connectionOpened(cnx)).exceptionally(this.HandleConnectionError);
			}
			catch (System.Exception t)
			{
				log.warn("[{}] [{}] Exception thrown while getting connection: ", state.topic, state.HandlerName, t);
				ReconnectLater(t);
			}
		}

		private void HandleConnectionError(System.Exception exception)
		{
			log.warn("[{}] [{}] Error connecting to broker: {}", this.state.topic, this.state.HandlerName, exception.Message);
			connection.ConnectionFailed(new PulsarClientException(exception.Message));

			State state = this.state.GetState();
			if (state == State.Uninitialized || state == State.Connecting || state == State.Ready)
			{
				ReconnectLater(exception);
			}
			return ;
		}

		protected internal virtual void ReconnectLater(System.Exception exception)
		{
			CLIENT_CNX_UPDATER.set(this, null);
			if (!ValidStateForReconnection)
			{
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.GetState());
				return;
			}
			long delayMs = backoff.Next();
			log.warn("[{}] [{}] Could not get connection to broker: {} -- Will try again in {} s", state.topic, state.HandlerName, exception.Message, delayMs / 1000.0);
			state.SetState(State.Connecting);
			state.client.timer().newTimeout(timeout =>
			{
			log.info("[{}] [{}] Reconnecting after connection was closed", state.topic, state.HandlerName);
			++epoch;
			grabCnx();
			}, delayMs, TimeUnit.MILLISECONDS);
		}
		
		public virtual void ConnectionClosed(ClientConnection cnx)
		{
			if (CLIENT_CNX_UPDATER.compareAndSet(this, cnx, null))
			{
				if (!ValidStateForReconnection)
				{
					log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.GetState());
					return;
				}
				long delayMs = backoff.Next();
				state.SetState(State.Connecting);
				log.info("[{}] [{}] Closed connection {} -- Will try again in {} s", state.topic, state.HandlerName, cnx.Channel(), delayMs / 1000.0);
				state.client.timer().newTimeout(timeout =>
				{
				log.info("[{}] [{}] Reconnecting after timeout", state.topic, state.HandlerName);
				++epoch;
				grabCnx();
				}, delayMs, TimeUnit.MILLISECONDS);
			}
		}

		protected internal virtual void ResetBackoff()
		{
			backoff.Reset();
		}

		protected internal virtual ClientConnection Cnx()
		{
			return CLIENT_CNX_UPDATER.get(this);
		}

		protected internal virtual bool isRetriableError(PulsarClientException e)
		{
			return e is PulsarClientException.LookupException;
		}


		public virtual ClientConnection ClientCnx
		{
			get
			{
				return CLIENT_CNX_UPDATER.get(this);
			}
			set
			{
				CLIENT_CNX_UPDATER.set(this, value);
			}
		}


		private bool ValidStateForReconnection
		{
			get
			{
				State state = this.state.GetState();
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

		public virtual long Epoch
		{
			get
			{
				return epoch;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConnectionHandler));
	}

}