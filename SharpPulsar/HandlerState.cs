using Akka.Actor;

namespace SharpPulsar
{
	
	public class HandlerState
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
		private readonly IActorRef _lookup;//get get reference to the actor that implements PulsarClientImpl
		private readonly string _topic;
		private readonly string _name;
		protected internal string Topic
        {
			get => _topic;
        }
		private readonly ActorSystem _system;

		private State _state;

		

		public HandlerState(IActorRef lookup, string topic, ActorSystem system, string name)
		{
			_lookup = lookup;
		    _topic = topic;
            _state = State.Uninitialized;
			_system = system;
			_name = name;
		}

		// moves the state to ready if it wasn't closed
		protected internal bool ChangeToReadyState()
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

		protected internal bool ChangeToRegisteringSchemaState()
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


		protected internal virtual string HandlerName {
			get => _name;
		}

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
		protected internal IActorRef Lookup
		{
			get
			{
				return _lookup;
			}
		}
	}

}
