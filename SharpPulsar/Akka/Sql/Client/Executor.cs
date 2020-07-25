using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Presto;
using SharpPulsar.Presto.Facebook.Type;

namespace SharpPulsar.Akka.Sql.Client
{
    public class Executor
	{
        private readonly ClientSession _clientSession;
        private readonly ClientOptions _clientOptions;
        private readonly IActorRef _handler;
        private readonly ILoggingAdapter _log;

        public Executor(ClientSession session, ClientOptions options, IActorRef handler, ILoggingAdapter log)
        {
            _clientSession = session;
            _clientOptions = options;
            _handler = handler;
            _log = log;
        }

		public bool Run()
		{
			var session = _clientSession;
            var queryRunner = new QueryRunner(session, _clientOptions.AccessToken, _clientOptions.User, _clientOptions.Password);
            return ExecuteCommand(queryRunner, _clientOptions.Execute);
        }

		private bool ExecuteCommand(QueryRunner queryRunner, string query)
		{
            return Process(queryRunner, query);
		}

		private bool Process(QueryRunner queryRunner, string sql)
		{
			try
			{
				var query = queryRunner.StartQuery(sql, _handler, _log);
				var success = query.MaterializeQueryOutput();

				var session = queryRunner.Session;

				// update catalog and schema if present
				if (!string.IsNullOrWhiteSpace(query.SetCatalog) || !string.IsNullOrWhiteSpace(query.SetSchema))
				{
					session = ClientSession.NewBuilder(session).WithCatalog(!string.IsNullOrWhiteSpace(query.SetCatalog)? query.SetCatalog : session.Catalog).WithSchema(!string.IsNullOrWhiteSpace(query.SetSchema)? query.SetSchema : session.Schema).Build();
					//schemaChanged.run();
				}

				// update transaction ID if necessary
				if (query.ClearTransactionId)
				{
					session = ClientSession.StripTransactionId(session);
				}

				var builder = ClientSession.NewBuilder(session);

				if (!string.IsNullOrWhiteSpace(query.StartedTransactionId))
				{
					builder = builder.WithTransactionId(query.StartedTransactionId);
				}

				// update session properties if present
				if (query.SetSessionProperties.Count > 0 || query.ResetSessionProperties.Count > 0)
				{
					IDictionary<string, string> sessionProperties = new Dictionary<string, string>(session.Properties);
                    sessionProperties = sessionProperties.Concat(query.SetSessionProperties).ToDictionary(x=> x.Key, x=> x.Value);
					query.ResetSessionProperties.ForEach(x=> sessionProperties.Remove(x));
					builder = builder.WithProperties(sessionProperties);
				}

				// update session roles
				if (query.SetRoles.Count > 0)
				{
					IDictionary<string, SelectedRole> roles = new Dictionary<string, SelectedRole>(session.Roles);
                    roles = roles.Concat(query.SetRoles).ToDictionary(x => x.Key, x => x.Value); 
					builder = builder.WithRoles(roles);
				}

				// update prepared statements if present
				if (query.AddedPreparedStatements.Count > 0 || query.DeallocatedPreparedStatements.Count > 0)
				{
					IDictionary<string, string> preparedStatements = new Dictionary<string, string>(session.PreparedStatements);
                    preparedStatements = preparedStatements.Concat(query.AddedPreparedStatements).ToDictionary(x => x.Key, x => x.Value); ;
					query.DeallocatedPreparedStatements.ForEach(x=> preparedStatements.Remove(x));
					builder = builder.WithPreparedStatements(preparedStatements);
				}

				session = builder.Build();
				queryRunner.Session = session;

				return success;
			}
			catch (Exception e)
			{
				_log.Error("Error running command: " + e.Message);
				return false;
			}
		}

	}
}
