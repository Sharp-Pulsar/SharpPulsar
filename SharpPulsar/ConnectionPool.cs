using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpPulsar
{
    public class ConnectionPool:ReceiveActor
    {
		private readonly Dictionary<EndPoint, Dictionary<int, IActorRef/*ClientCnx*/>> _pool;

		private readonly ClientConfigurationData _clientConfig;
		private readonly int _maxConnectionsPerHosts;
		private readonly bool _isSniProxy;
		private readonly ILoggingAdapter _log;
		public ConnectionPool(ClientConfigurationData conf)
		{
			_log = Context.GetLogger();
			_clientConfig = conf;
			_maxConnectionsPerHosts = conf.ConnectionsPerBroker;
			_isSniProxy = _clientConfig.UseTls && _clientConfig.ProxyProtocol != null && !string.IsNullOrWhiteSpace(_clientConfig.ProxyServiceUrl);

			_pool = new Dictionary<EndPoint, Dictionary<int, IActorRef>>();
			Receive<GetConnectionForAddress>(m => {
				var cnx = GetConnection(m.EndPoint, m.TargetBroker);
				Sender.Tell(cnx);
			});
		}
		private readonly Random _random = new Random();


		/// <summary>
		/// Get a connection from the pool.
		/// <para>
		/// The connection can either be created or be coming from the pool itself.
		/// </para>
		/// <para>
		/// When specifying multiple addresses, the logicalAddress is used as a tag for the broker, while the physicalAddress
		/// is where the connection is actually happening.
		/// </para>
		/// <para>
		/// These two addresses can be different when the client is forced to connect through a proxy layer. Essentially, the
		/// pool is using the logical address as a way to decide whether to reuse a particular connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="logicalAddress">
		///            the address to use as the broker tag </param>
		/// <param name="physicalAddress">
		///            the real address where the TCP connection should be made </param>
		/// <returns> a future that will produce the ClientCnx object </returns>
		private IActorRef GetConnection(DnsEndPoint address, string targetBroker)
		{
			if (_maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return CreateConnection(address, targetBroker, -1);
			}

			int randomKey = SignSafeMod(_random.Next(), _maxConnectionsPerHosts);
			if(_pool.TryGetValue(address, out var cnx))
            {
				if (cnx.TryGetValue(randomKey, out var cn))
					return cn;
				var connection = CreateConnection(address, targetBroker, randomKey);
				_pool[address][randomKey] = connection;
				return connection;
            }
            else
			{
				var connection = CreateConnection(address, targetBroker, randomKey);
				_pool.Add(address, new Dictionary<int, IActorRef> { { randomKey, connection } });
				return connection;

			}}
		private IActorRef CreateConnection(DnsEndPoint address, string targetBroker, int connectionKey)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Connection for {address.Host} not found in cache");
			}
			return Context.ActorOf(ClientCnx.Prop(_clientConfig, address, targetBroker), connectionKey.ToString());			
		}
		private void CleanupConnection(EndPoint address, int connectionKey)
		{
			if (_pool.TryGetValue(address, out var map))
			{
				if(map.TryGetValue(connectionKey, out var m))
                {
					m.GracefulStop(TimeSpan.FromSeconds(5));
				}
				map.Remove(connectionKey);
			}
		}
		private int PoolSize
		{
			get
			{
				return _pool.Values.Select(x=> x.Values.Count).Sum();
			}
		}

		private int SignSafeMod(long dividend, int divisor)
		{
			int mod = (int)(dividend % divisor);
			if (mod < 0)
			{
				mod += divisor;
			}
			return mod;
		}
	}
}
