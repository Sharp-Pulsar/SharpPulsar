using SharpPulsar.Exception;
using System;
using System.Collections.Concurrent;

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
	public interface IConnection
	{
		void ConnectionFailed(PulsarClientException Exception);
		void ConnectionOpened(ClientCnx Cnx);
	}

	public class ConnectionHandler
	{
		private static readonly ConcurrentDictionary<ConnectionHandler, ClientCnx> ClientCnxUpdater = new ConcurrentDictionary<ConnectionHandler, ClientCnx>();

		private volatile ClientCnx _clientCnx = null;

		protected internal readonly HandlerState State;
		protected internal readonly Backoff Backoff;
		protected internal long EpochConflict = 0L;

		

		protected internal IConnection Connection;

		public ConnectionHandler(HandlerState State, Backoff Backoff, IConnection Connection)
		{
			this.State = State;
			this.Connection = Connection;
			this.Backoff = Backoff;
			ClientCnxUpdater.TryAdd(this, null);
		}

		public virtual void GrabCnx()
		{
			
			if (ClientCnxUpdater.TryGetValue(this, out var clientCnx))
			{
				if(clientCnx != null)
				{
					log.warn("[{}] [{}] Client cnx already set, ignoring reconnection request", State.Topic, State.HandlerName);
					return;
				}
				
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", this.State.Topic, State.HandlerName, State.GetState());
				return;
			}

			try
			{
				State.Client.GetConnection(State.Topic).thenAccept(cnx => Connection.connectionOpened(cnx)).exceptionally(this.handleConnectionError);
			}
			catch (System.Exception T)
			{
				log.warn("[{}] [{}] Exception thrown while getting connection: ", State.Topic, State.HandlerName, T);
				ReconnectLater(T);
			}
		}

		private void HandleConnectionError(Exception Exception)
		{
			log.warn("[{}] [{}] Error connecting to broker: {}", State.topic, State.HandlerName, Exception.Message);
			Connection.connectionFailed(new PulsarClientException(Exception));

			State State = this.State.getState();
			if (State == State.Uninitialized || State == State.Connecting || State == State.Ready)
			{
				ReconnectLater(Exception);
			}

			return null;
		}

		public virtual void ReconnectLater(Exception Exception)
		{
			CLIENT_CNX_UPDATER.set(this, null);
			if (!ValidStateForReconnection)
			{
				log.info("[{}] [{}] Ignoring reconnection request (state: {})", State.topic, State.HandlerName, State.getState());
				return;
			}
			long DelayMs = Backoff.next();
			log.warn("[{}] [{}] Could not get connection to broker: {} -- Will try again in {} s", State.topic, State.HandlerName, Exception.Message, DelayMs / 1000.0);
			State.setState(State.Connecting);
			State.client.timer().newTimeout(timeout =>
			{
			log.info("[{}] [{}] Reconnecting after connection was closed", State.topic, State.HandlerName);
			++EpochConflict;
			GrabCnx();
			}, DelayMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		public virtual void ConnectionClosed(ClientCnx Cnx)
		{
			if (CLIENT_CNX_UPDATER.compareAndSet(this, Cnx, null))
			{
				if (!ValidStateForReconnection)
				{
					log.info("[{}] [{}] Ignoring reconnection request (state: {})", State.topic, State.HandlerName, State.getState());
					return;
				}
				long DelayMs = Backoff.next();
				State.setState(State.Connecting);
				log.info("[{}] [{}] Closed connection {} -- Will try again in {} s", State.topic, State.HandlerName, Cnx.channel(), DelayMs / 1000.0);
				State.client.timer().newTimeout(timeout =>
				{
				log.info("[{}] [{}] Reconnecting after timeout", State.topic, State.HandlerName);
				++EpochConflict;
				GrabCnx();
				}, DelayMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
		}

		public virtual void ResetBackoff()
		{
			Backoff.reset();
		}

		public virtual ClientCnx Cnx()
		{
			return CLIENT_CNX_UPDATER.get(this);
		}

		public virtual bool IsRetriableError(PulsarClientException E)
		{
			return E is PulsarClientException.LookupException;
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
				State State = this.State.getState();
				switch (State)
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
				return EpochConflict;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConnectionHandler));
	}

}