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

	public abstract class HandlerState
	{
        protected internal readonly string Topic;

		private static readonly ConcurrentDictionary<HandlerState, State> StateUpdater = new ConcurrentDictionary<HandlerState, State>();
		
		public enum State
		{
			Uninitialized, // Not initialized
			Connecting, // Client connecting to broker
			Ready, // Handler is being used
			Closing, // Close cmd has been sent to broker
			Closed, // Broker acked the close
			Terminated, // Topic associated with this handler
						// has been terminated
			Failed, // Handler is failed
			RegisteringSchema // Handler is registering schema
		}

        protected HandlerState(PulsarClientImpl client, string topic)
		{
			Client = client;
			Topic = topic;
			StateUpdater[this] =  State.Uninitialized;
		}

		// moves the state to ready if it wasn't closed
		public virtual bool ChangeToReadyState()
		{
			return (StateUpdater.TryUpdate(this, State.Ready, State.Uninitialized) || StateUpdater.TryUpdate(this, State.Ready, State.Connecting) || StateUpdater.TryUpdate(this, State.Ready, State.RegisteringSchema));
		}
        public  void ChangeToState(State state)
        {
            StateUpdater[this] = state;
        }
		public virtual bool ChangeToRegisteringSchemaState()
		{
			return StateUpdater.TryUpdate(this, State.RegisteringSchema, State.Ready);
		}

		public virtual State? GetState()
		{
			return StateUpdater[this];
		}

		public void SetState(State s)
		{
			StateUpdater[this] =  s;
		}

		public string HandlerName { get; set; }

		public virtual State GetAndUpdateState(in System.Func<State, State> updater)
		{
			var oldState = StateUpdater[this];
			var newState = updater.Invoke(oldState);
			if (StateUpdater.TryUpdate(this, newState, oldState))
				return newState;
			return State.Uninitialized;
		}

		public PulsarClientImpl Client { get; set; }
    }

}