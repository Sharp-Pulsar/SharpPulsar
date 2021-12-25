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
using System.Threading.Tasks;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar
{
	public class ConnectionPool : ReceiveActor, IWithUnboundedStash
	{
		private readonly Dictionary<EndPoint, Dictionary<int, ConnectionOpened>> _pool;

		private readonly ClientConfigurationData _clientConfig;
		private readonly int _maxConnectionsPerHosts;
		private readonly ILoggingAdapter _log;
		private readonly IActorContext _context;
		private IActorRef _replyTo;
		private int _randomKey;
		private DnsEndPoint _logicalEndpoint;
		public ConnectionPool(ClientConfigurationData conf)
		{
			_context = Context;
			_log = Context.GetLogger();
			_clientConfig = conf;
			_maxConnectionsPerHosts = conf.ConnectionsPerBroker;
			//_isSniProxy = _clientConfig.UseTls && _clientConfig.ProxyProtocol != null && !string.IsNullOrWhiteSpace(_clientConfig.ProxyServiceUrl);

			_pool = new Dictionary<EndPoint, Dictionary<int, ConnectionOpened>>();
			Listen();
		}
		private void Listen()
        {
			ReceiveAsync<GetConnection>(async g =>
			{
                try
                {
                    ConnectionOpened connection = null;
                    _randomKey = SignSafeMod(Random.Next(), _maxConnectionsPerHosts);
                    _logicalEndpoint = g.LogicalEndPoint;
                    if (g.LogicalEndPoint != null && g.PhusicalEndPoint == null)
                    {
                        connection = await GetConnection(g.LogicalEndPoint, _randomKey);
                    }
                    else if (g.LogicalEndPoint != null && g.PhusicalEndPoint != null)
                    {
                        connection = await GetConnection(g.LogicalEndPoint, g.PhusicalEndPoint, _randomKey);
                    }
                    else
                    {
                        connection = await GetConnection(g.LogicalEndPoint, _randomKey);
                    }
                    Sender.Tell(new AskResponse(connection));
                }
                catch (Exception e)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
                }
            });
			Receive<CleanupConnection>(c =>
			{
				CleanupConnection(c.Address, c.ConnectionKey);
			});
			Receive<CloseAllConnections>(_ =>
			{
				CloseAllConnections();
			});
			Receive<ReleaseConnection>(c =>
			{
				ReleaseConnection(c.ClientCnx);
			});
			Receive<GetPoolSize>(c =>
			{
				Sender.Tell(new GetPoolSizeResponse(PoolSize));
			});
			Stash?.UnstashAll();
		}
		public static Props Prop(ClientConfigurationData conf)
		{
			return Props.Create(() => new ConnectionPool(conf));
		}
		private static readonly Random Random = new Random();


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
		private async ValueTask<ConnectionOpened> GetConnection(DnsEndPoint address, int randomKey)
		{
			return await GetConnection(address, address, randomKey);
		}
		private async ValueTask<ConnectionOpened> GetConnection(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress, int randomKey)
		{
			if (_maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return await CreateConnection(logicalAddress, physicalAddress, -1);
			}

			if (_pool.TryGetValue(logicalAddress, out var cnx))
			{
				if (cnx.TryGetValue(randomKey, out var cn))
					return cn;

				return await CreateConnection(logicalAddress, physicalAddress, randomKey);
			}
            return await CreateConnection(logicalAddress, physicalAddress, randomKey);
        }
		private async ValueTask<ConnectionOpened> CreateConnection(DnsEndPoint logicalAddress, DnsEndPoint physicalAddress, int connectionKey)
		{
            IActorRef cnx = ActorRefs.NoSender;
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"Connection for {logicalAddress} not found in cache");
                }
                var targetBroker = $"{physicalAddress.Host}:{physicalAddress.Port}";

                if (!logicalAddress.Equals(physicalAddress))
                    targetBroker = $"{logicalAddress.Host}:{logicalAddress.Port}";
                var tcs = new TaskCompletionSource<ConnectionOpened>();
                cnx = _context.ActorOf(ClientCnx.Prop(_clientConfig, physicalAddress, tcs, targetBroker), $"{targetBroker}{connectionKey}".ToAkkaNaming());
                var connection = await tcs.Task;
                
                if (_pool.TryGetValue(_logicalEndpoint, out _))
                {
                    _pool[_logicalEndpoint][_randomKey] = connection;
                }
                else
                {
                    _pool.Add(_logicalEndpoint, new Dictionary<int, ConnectionOpened> { { _randomKey, connection } });
                }

                return connection;
            }
            catch
            {
                Context.Stop(cnx);
                await Task.Delay(TimeSpan.FromSeconds(1));
                throw;
            }
        }
		private void CleanupConnection(DnsEndPoint address, int connectionKey)
		{
			if (_pool.TryGetValue(address, out var map))
			{
				if (map.TryGetValue(connectionKey, out var m))
				{
					m.ClientCnx.GracefulStop(TimeSpan.FromSeconds(5));
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
					c.ClientCnx.GracefulStop(TimeSpan.FromSeconds(1));
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
				return _pool.Values.Select(x => x.Values.Count).Sum();
			}
		}

        public IStash Stash { get; set; }

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
