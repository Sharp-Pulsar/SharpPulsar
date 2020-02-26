using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using DotNetty.Common.Utilities;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Network;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
		void ConnectionFailed(PulsarClientException exception);
		void ConnectionOpened(ClientCnx cnx);
	}

	public class ConnectionHandler
	{
		private static readonly ConcurrentDictionary<ConnectionHandler, ClientConnection> ClientCnxUpdater = new ConcurrentDictionary<ConnectionHandler, ClientConnection>();

		//private volatile ClientCnx _clientCnx = null;

		protected internal readonly HandlerState State;
		public readonly Backoff Backoff;
		protected internal long _epoch = 0L;

		

		protected internal IConnection Connection;

		public ConnectionHandler(HandlerState state, Backoff backoff, IConnection connection)
		{
			State = state;
			Connection = connection;
			Backoff = backoff;
			ClientCnxUpdater.TryAdd(this, null);
		}

		public virtual void GrabCnx()
		{
			
			if (ClientCnxUpdater.TryGetValue(this, out var clientCnx))
			{
				if(clientCnx != null)
				{
					Log.LogWarning("[{}] [{}] Client cnx already set, ignoring reconnection request", State.Topic, State.HandlerName);
					return;
				}
				
			}

			if (!ValidStateForReconnection)
			{
				// Ignore connection closed when we are shutting down
				Log.LogInformation("[{}] [{}] Ignoring reconnection request (state: {})", State.Topic, State.HandlerName, State.GetState());
				return;
			}

            try
            {
                State.Client.GetConnection(State.Topic).AsTask().ContinueWith(cnx =>
                {
                    if (cnx.IsFaulted)
                        HandleConnectionError(cnx.Exception);
                    else 
                        Connection.ConnectionOpened(cnx.Result);
                });
            }
            catch (Exception T)
            {
                Log.LogWarning("[{}] [{}] Exception thrown while getting connection: ", State.Topic, State.HandlerName,
                    T);
                ReconnectLater(T);
            }
        }

		private void HandleConnectionError(Exception exception)
		{
			Log.LogWarning("[{}] [{}] Error connecting to broker: {}", State.Topic, State.HandlerName, exception.Message);
			Connection.ConnectionFailed(new PulsarClientException(exception.Message));

			var stte = State.GetState();
			if (stte == HandlerState.State.Uninitialized || stte == HandlerState.State.Connecting || stte == HandlerState.State.Ready)
			{
				ReconnectLater(exception);
			}
		}

		public virtual void ReconnectLater(Exception exception)
		{
			ClientCnxUpdater[this] =  null;
			if (!ValidStateForReconnection)
			{
				Log.LogInformation("[{}] [{}] Ignoring reconnection request (state: {})", State.Topic, State.HandlerName, State.GetState());
				return;
			}
			var delayMs = Backoff.Next();
			Log.LogWarning("[{}] [{}] Could not get connection to broker: {} -- Will try again in {} s", State.Topic, State.HandlerName, exception.Message, delayMs / 1000.0);
			State.SetState(HandlerState.State.Connecting);
			State.Client.Timer.NewTimeout(new ReconnectAfterTimeout(this), TimeSpan.FromMilliseconds(delayMs));
		}

        private class ReconnectAfterTimeout : ITimerTask
        {
            private readonly ConnectionHandler _outerInstance;
			public ReconnectAfterTimeout(ConnectionHandler outerInstance)
            {
                _outerInstance = outerInstance;
            }
            public void Run(ITimeout timeout)
            {
				if(timeout.Canceled)
					return;
				Log.LogInformation("[{}] [{}] Reconnecting after timeout", _outerInstance.State.Topic, _outerInstance.State.HandlerName);
                _outerInstance._epoch++;
                _outerInstance.GrabCnx();
			}
        }
		public virtual void ConnectionClosed(ClientConnection cnx)
		{
			if (ClientCnxUpdater.TryUpdate(this, cnx, null))
			{
				if (!ValidStateForReconnection)
				{
					Log.LogInformation("[{}] [{}] Ignoring reconnection request (state: {})", State.Topic, State.HandlerName, State.GetState());
					return;
				}
				var delayMs = Backoff.Next();
				State.SetState(HandlerState.State.Connecting);
				Log.LogInformation("[{}] [{}] Closed connection {} -- Will try again in {} s", State.Topic, State.HandlerName, cnx.Channel(), delayMs / 1000.0);
				State.Client.Timer.NewTimeout(new ReconnectAfterTimeout(this), TimeSpan.FromMilliseconds(delayMs));
			}
		}

		public virtual void ResetBackoff()
		{
			Backoff.Reset();
		}

		public virtual ClientConnection Cnx()
		{
			return ClientCnxUpdater[this];
		}

		public virtual bool IsRetriableError(PulsarClientException e)
		{
			return e is PulsarClientException.LookupException;
		}

		public virtual ClientConnection ClientCnx
		{
			get => ClientCnxUpdater[this];
            set => ClientCnxUpdater[this] =  value;
        }


		private bool ValidStateForReconnection
		{
			get
            {
                var state = State.GetState();
                return state switch
                {
                    HandlerState.State.Uninitialized =>
                    // Ok
                    true,
                    HandlerState.State.Connecting =>
                    // Ok
                    true,
                    HandlerState.State.Ready =>
                    // Ok
                    true,
                    HandlerState.State.Closing => false,
                    HandlerState.State.Closed => false,
                    HandlerState.State.Failed => false,
                    HandlerState.State.Terminated => false,
                    _ => false
                };
            }
		}

		public virtual long Epoch => _epoch;
        private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<ConnectionHandler>();
	}

}