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
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly PulsarClientImpl ClientConflict;
		protected internal readonly string Topic;

		private static readonly AtomicReferenceFieldUpdater<HandlerState, State> STATE_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(HandlerState), typeof(State), "state");
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile State state = null;
		private volatile State state = null;

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

		public HandlerState(PulsarClientImpl Client, string Topic)
		{
			this.ClientConflict = Client;
			this.Topic = Topic;
			STATE_UPDATER.set(this, State.Uninitialized);
		}

		// moves the state to ready if it wasn't closed
		public virtual bool ChangeToReadyState()
		{
			return (STATE_UPDATER.compareAndSet(this, State.Uninitialized, State.Ready) || STATE_UPDATER.compareAndSet(this, State.Connecting, State.Ready) || STATE_UPDATER.compareAndSet(this, State.RegisteringSchema, State.Ready));
		}

		public virtual bool ChangeToRegisteringSchemaState()
		{
			return STATE_UPDATER.compareAndSet(this, State.Ready, State.RegisteringSchema);
		}

		public virtual State? GetState()
		{
			return STATE_UPDATER.get(this);
		}

		public virtual void SetState(State S)
		{
			STATE_UPDATER.set(this, S);
		}

		public abstract string HandlerName {get;}

		public virtual State? GetAndUpdateState(in System.Func<State, State> Updater)
		{
			return STATE_UPDATER.getAndUpdate(this, Updater);
		}

		public virtual PulsarClientImpl Client
		{
			get
			{
				return ClientConflict;
			}
		}
	}

}