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

	internal abstract class HandlerState
	{
		protected internal readonly PulsarClientImpl client;
		protected internal readonly string topic;

		private static readonly AtomicReference<HandlerState, State> STATE_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(HandlerState), typeof(State), "state");

		private readonly State _state;

		internal enum State
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

		public HandlerState(PulsarClientImpl client, string topic)
		{
			this.client = client;
			this.topic = topic;
			STATE_UPDATER.set(this, State.Uninitialized);
		}

		// moves the state to ready if it wasn't closed
		protected internal virtual bool ChangeToReadyState()
		{
			return (STATE_UPDATER.compareAndSet(this, State.Uninitialized, State.Ready) || STATE_UPDATER.compareAndSet(this, State.Connecting, State.Ready) || STATE_UPDATER.compareAndSet(this, State.RegisteringSchema, State.Ready));
		}

		protected internal virtual bool ChangeToRegisteringSchemaState()
		{
			return STATE_UPDATER.compareAndSet(this, State.Ready, State.RegisteringSchema);
		}

		protected internal virtual State GetState()
		{
			return STATE_UPDATER.get(this);
		}

		protected internal virtual void SetState(State s)
		{
			STATE_UPDATER.set(this, s);
		}

		internal abstract string HandlerName {get;}
		protected internal virtual State GetAndUpdateState(System.Func<State, State> updater)
		{
			return STATE_UPDATER.getAndUpdate(this, updater);
		}

		public virtual PulsarClientImpl Client
		{
			get
			{
				return client;
			}
		}
	}

}