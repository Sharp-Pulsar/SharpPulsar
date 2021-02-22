using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Messages.Client;
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
		private readonly ILoggingAdapter _log;
		private readonly IActorContext _context;
		public ConnectionPool(ClientConfigurationData conf)
		{
			_context = Context;
			_log = Context.GetLogger();
			_clientConfig = conf;
			_maxConnectionsPerHosts = conf.ConnectionsPerBroker;
			//_isSniProxy = _clientConfig.UseTls && _clientConfig.ProxyProtocol != null && !string.IsNullOrWhiteSpace(_clientConfig.ProxyServiceUrl);

			_pool = new Dictionary<EndPoint, Dictionary<int, IActorRef>>();
			Receive<GetConnection>(c => 
			{
				IActorRef connection = ActorRefs.Nobody;
				if(c.LogicalEndPoint != null && c.PhusicalEndPoint == null)
                {
					connection = GetConnection(c.LogicalEndPoint);
                }
				else if(c.LogicalEndPoint != null && c.PhusicalEndPoint != null)
					connection = GetConnection(c.LogicalEndPoint, c.PhusicalEndPoint);


				Sender.Tell(new GetConnectionResponse(connection));

			});
			Receive<CleanupConnection>(c =>
			{
				CleanupConnection(c.Address, c.ConnectionKey);
			});
			Receive<CloseAllConnections>(_ =>
			{
				CloseAllConnections();
			});
			Receive<ConnectionOpened>(_ =>
			{
				//CloseAllConnections();
			});
			Receive<ReleaseConnection>(c =>
			{
				ReleaseConnection(c.ClientCnx);
			});
			Receive<GetPoolSize>(c =>
			{
				Sender.Tell(new GetPoolSizeResponse(PoolSize));
			});
		}

		public static Props Prop(ClientConfigurationData conf)
        {
			return Props.Create(() => new ConnectionPool(conf));
        }
		private static readonly Random _random = new Random();


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
		private IActorRef GetConnection(DnsEndPoint address)
        {
			return GetConnection(address, address);
        }
		private IActorRef GetConnection(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress)
		{
			if (_maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return CreateConnection(logicalAddress, physicalAddress, -1);
			}

			int randomKey = SignSafeMod(_random.Next(), _maxConnectionsPerHosts);
			if(_pool.TryGetValue(logicalAddress, out var cnx))
            {
				if (cnx.TryGetValue(randomKey, out var cn))
					return cn;
				var connection = CreateConnection(logicalAddress, physicalAddress, randomKey);
				_pool[logicalAddress][randomKey] = connection;
				return connection;
            }
            else
			{
				var connection = CreateConnection(logicalAddress, physicalAddress, randomKey);
				_pool.Add(logicalAddress, new Dictionary<int, IActorRef> { { randomKey, connection } });
				return connection;

			}}
		private IActorRef CreateConnection(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress, int connectionKey)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Connection for {logicalAddress} not found in cache");
			}
			string targetBroker = string.Empty;

			if (!logicalAddress.Equals(physicalAddress))
				targetBroker = $"{logicalAddress.Host}:{logicalAddress.Port}";

			return _context.ActorOf(ClientCnx.Prop(_clientConfig, physicalAddress, targetBroker), $"{targetBroker}{connectionKey}".ToAkkaNaming());			
		}
		private void CleanupConnection(DnsEndPoint address, int connectionKey)
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
		private void CloseAllConnections()
		{
			_pool.Values.ForEach(map =>
			{
				map.Values.ForEach(c =>
				{
					c.GracefulStop(TimeSpan.FromSeconds(1));
				});
			});
		}
		private void ReleaseConnection(IActorRef cnx)
		{
			if (_maxConnectionsPerHosts == 0)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug("close connection due to pooling disabled.");
				}
				cnx.GracefulStop(TimeSpan.FromSeconds(5));
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
