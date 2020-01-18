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

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using State = SharpPulsar.Impl.HandlerState.State;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ConnectionHandler
	{
		private static readonly AtomicReferenceFieldUpdater<ConnectionHandler, ClientCnx> CLIENT_CNX_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(ConnectionHandler), typeof(ClientCnx), "clientCnx");

		private volatile ClientCnx clientCnx = null;

		protected internal readonly HandlerState state;
		protected internal readonly Backoff backoff;
		protected internal long epoch = 0L;

		internal interface Connection
		{
			void connectionFailed(PulsarClientException exception);
			void connectionOpened(ClientCnx cnx);
		}

		protected internal Connection connection;

		protected internal ConnectionHandler(HandlerState state, Backoff backoff, Connection connection)
		{
			this.state = state;
			this.connection = connection;
			this.backoff = backoff;
			CLIENT_CNX_UPDATER.set(this, null);
		}

		protected internal virtual void grabCnx()
		{
			if (CLIENT_CNX_UPDATER.get(this) != null)
			{
				log.warn("[{}] [{}] Client cnx already set, ignoring reconnection request", state.topic, state.HandlerName);
				return;
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.getState());
				return;
			}

			try
			{
				state.client.getConnection(state.topic).thenAccept(cnx => connection.connectionOpened(cnx)).exceptionally(this.handleConnectionError);
			}
			catch (Exception t)
			{
				log.warn("[{}] [{}] Exception thrown while getting connection: ", state.topic, state.HandlerName, t);
				reconnectLater(t);
			}
		}

		private Void handleConnectionError(Exception exception)
		{
			log.warn("[{}] [{}] Error connecting to broker: {}", state.topic, state.HandlerName, exception.Message);
			connection.connectionFailed(new PulsarClientException(exception));

			State state = this.state.getState();
			if (state == State.Uninitialized || state == State.Connecting || state == State.Ready)
			{
				reconnectLater(exception);
			}

			return null;
		}

		protected internal virtual void reconnectLater(Exception exception)
		{
			CLIENT_CNX_UPDATER.set(this, null);
			if (!ValidStateForReconnection)
			{
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.getState());
				return;
			}
			long delayMs = backoff.next();
			log.warn("[{}] [{}] Could not get connection to broker: {} -- Will try again in {} s", state.topic, state.HandlerName, exception.Message, delayMs / 1000.0);
			state.setState(State.Connecting);
			state.client.timer().newTimeout(timeout =>
			{
			log.info("[{}] [{}] Reconnecting after connection was closed", state.topic, state.HandlerName);
			++epoch;
			grabCnx();
			}, delayMs, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public void connectionClosed(ClientCnx cnx)
		public virtual void connectionClosed(ClientCnx cnx)
		{
			if (CLIENT_CNX_UPDATER.compareAndSet(this, cnx, null))
			{
				if (!ValidStateForReconnection)
				{
					log.info("[{}] [{}] Ignoring reconnection request (state: {})", state.topic, state.HandlerName, state.getState());
					return;
				}
				long delayMs = backoff.next();
				state.setState(State.Connecting);
				log.info("[{}] [{}] Closed connection {} -- Will try again in {} s", state.topic, state.HandlerName, cnx.channel(), delayMs / 1000.0);
				state.client.timer().newTimeout(timeout =>
				{
				log.info("[{}] [{}] Reconnecting after timeout", state.topic, state.HandlerName);
				++epoch;
				grabCnx();
				}, delayMs, TimeUnit.MILLISECONDS);
			}
		}

		protected internal virtual void resetBackoff()
		{
			backoff.reset();
		}

		protected internal virtual ClientCnx cnx()
		{
			return CLIENT_CNX_UPDATER.get(this);
		}

		protected internal virtual bool isRetriableError(PulsarClientException e)
		{
			return e is PulsarClientException.LookupException;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public ClientCnx getClientCnx()
		public virtual ClientCnx ClientCnx
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
				State state = this.state.getState();
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public long getEpoch()
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