using Akka.Actor;

namespace SharpPulsar
{
	
	public abstract class HandlerState
	{
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
		private readonly IActorRef _client;//get get reference to the actor that implements PulsarClientImpl
		private readonly string _topic;
		protected internal string Topic
        {
			get => _topic;
        }
		private readonly ActorSystem _system;

		private State _state;

		

		public HandlerState(IActorRef client, string topic, ActorSystem system)
		{
			_client = client;
		    _topic = topic;
            _state = State.Uninitialized;
			_system = system;
		}

		// moves the state to ready if it wasn't closed
		protected bool ChangeToReadyState()
		{
            switch (_state)
            {
				case State.Uninitialized:
				case State.Connecting:
				case State.RegisteringSchema:
					_state = State.Ready;
					return true;
				default:
					return false;
            }
		}

		protected bool ChangeToRegisteringSchemaState()
		{
			if(_state == State.Ready)
            {
				_state = State.RegisteringSchema;
				return true;
            }
			return false;
		}

		protected internal State ConnectionState
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}


		internal abstract string HandlerName { get; }

		protected internal State GetAndUpdateState(State stateUpdate)
		{
			var state = _state;
			_state = stateUpdate;
			return state;
		}
		protected internal ActorSystem System
        {
			get => _system;
        }
		protected internal IActorRef Client
		{
			get
			{
				return _client;
			}
		}
	}

}
