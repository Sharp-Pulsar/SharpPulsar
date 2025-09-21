using System;
using Akka.Actor;
using SharpPulsar.Client;

namespace SharpPulsar
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
        RegisteringSchema, // Handler is registering schema
        ProducerFenced, // The producer has been fenced by the broker
    }
    
    public class HandlerStateActor : ReceiveActor
    {
        private Uri _redirectedClusterURI;
        private readonly IActorRef _lookup;
        private readonly IActorRef _connectionPool;
        private readonly string _topic;
        private readonly string _name;
        private readonly IActorRef _client;
        
        
        private State _state;
        public HandlerStateActor(IActorRef client, IActorRef lookup, IActorRef connectionPool, string topic, string name)
        {
            _client = client;
            _connectionPool = connectionPool;
            _lookup = lookup;
            _topic = topic;
            _state = State.Uninitialized;
            _name = name;
            Receive<SetRedirectedCluster>(set =>
            {
                SetRedirectedClusterURI(set.ServiceUrl, set.ServiceUrlTls);
            });
            Receive<GetRedirectedCluster>(_ =>
            {
                Sender.Tell(_redirectedClusterURI);
            });
            Receive<ChangeToReadyState>(set =>
            {
                var b = ChangeToReadyState();
                Sender.Tell(b);
            });
            Receive<ChangeToRegisteringSchemaState>(_ => 
            {
                Sender.Tell(ChangeToRegisteringSchemaState());
            });
            Receive<ChangeToConnecting>(_ =>
            {
                Sender.Tell(ChangeToConnecting());
            });
            Receive<GetState>(_ =>
            {
                Sender.Tell(_state);
            });
            Receive<SetState>(s =>
            {
                _state = s.State;
            });
            Receive<GetHandlerName>(_ =>
            {
                Sender.Tell(_name);
            });
            Receive<GetAndUpdateState>(update =>
            {
                _state = update.State;  
                Sender.Tell(_state);
            });
            Receive<GetClient>(_ =>
            {
                Sender.Tell(_client);
            });
            Receive<Lookup>(_ =>
            {
                Sender.Tell(_lookup);
            });
            Receive<ConnectionPool>(_ =>
            {
                Sender.Tell(_connectionPool);
            });
            Receive<GetAll>(_ =>
            {
                Sender.Tell(new HandlerAll(Self, _state, _topic, _name, _lookup, _connectionPool, _redirectedClusterURI));
            });

        }
        public static Props Prop(IActorRef client, IActorRef lookup, IActorRef connectionPool, string topic, string name)
        {
            return Props.Create(() => new HandlerStateActor(client, lookup, connectionPool, topic, name));
        }
        private void SetRedirectedClusterURI(string serviceUrl, string serviceUrlTls)
        {
            string url = _client.Ask<ClientConfiguration>(ClientConfiguration.Instance).GetAwaiter().GetResult().Configuration.UseTls && string.IsNullOrWhiteSpace(serviceUrlTls) ? serviceUrlTls : serviceUrl;
            _redirectedClusterURI = new Uri(url);
        }
        private bool ChangeToReadyState()
        {
            if (_state == State.Ready)
                return true;
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
        private bool ChangeToRegisteringSchemaState()
        {
            if (_state == State.Ready)
            {
                _state = State.RegisteringSchema;
                return true;
            }
            return false;
        }
        private  bool ChangeToConnecting()
        {
            if (_state == State.Connecting)
            {
                return true;
            }
            switch (_state)
            {
                case State.Uninitialized:
                case State.Ready:
                case State.RegisteringSchema:
                    _state = State.Connecting;
                    return true;
                default:
                    return false;
            }
           
        }
    }
    
    public record struct ChangeToReadyState
    {
        public static ChangeToReadyState Instance = new ChangeToReadyState(); 
    }
    public record struct SetRedirectedCluster(string ServiceUrl, string ServiceUrlTls)
    {
        public static SetRedirectedCluster Instance = new SetRedirectedCluster();
    }
    public record struct GetRedirectedCluster()
    {
        public static GetRedirectedCluster Instance = new GetRedirectedCluster();
    }
    public record struct ChangeToRegisteringSchemaState
    {
        public static ChangeToRegisteringSchemaState Instance  = new ChangeToRegisteringSchemaState();
    }
    public record struct GetState
    {
        public static GetState Instance = new GetState();
    }
    public record struct ChangeToConnecting
    {
        public static ChangeToConnecting Instance = new ChangeToConnecting();
    }
    public record struct GetHandlerName
    {
        public static GetHandlerName Instance = new GetHandlerName();
    }
    public record struct GetAndUpdateState(State State);
    public record struct GetClient
    {
        public static GetClient Instance = new GetClient();
    }
    public record struct SetState(State State);
    public record struct Lookup
    {
        public static Lookup Instance = new Lookup();
    }
    public record struct ConnectionPool
    {
        public static ConnectionPool Instance = new ConnectionPool();
    }
    public record struct GetAll
    {
        public static GetAll Instance = new GetAll();
    }
    public record struct HandlerAll(IActorRef StateActor, State State, string Topic, string HandlerName, IActorRef Lookup, IActorRef ConnectionPool, Uri RedirectedClusterURI);
}
