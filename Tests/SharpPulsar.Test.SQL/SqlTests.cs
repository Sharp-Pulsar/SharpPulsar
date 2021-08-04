using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Test.SQL.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Public;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.SQL
{
	[Collection(nameof(PulsarSqlTests))]
	public class SqlTests
	{
		private readonly ITestOutputHelper _output;
		public SqlTests(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
		}
		[Fact(Skip ="Issue with sql-worker on github action")]
		//[Fact]
		public virtual void TestQuerySql()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
            Thread.Sleep(TimeSpan.FromSeconds(30));
			var sql = Sql<SqlData>.NewSql(null);
			var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
			var query = new SqlQuery(option, e => { Console.WriteLine(e.ToString()); }, Console.WriteLine);
			sql.SendQuery(query);
			var receivedCount = 0;
            var response = sql.ReadQueryResult(TimeSpan.FromSeconds(30));
            var data = response.Response;
            switch (data)
            {
                case DataResponse dr:
                    {
                        for(var i = 0; i < dr.Data.Count; i++)
                        {
                            var ob = dr.Data.ElementAt(i)["text"].ToString();
                            _output.WriteLine(ob);
                            receivedCount++;
                        }
                        _output.WriteLine(JsonSerializer.Serialize(dr.StatementStats, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    break;
                case StatsResponse sr:
                    _output.WriteLine(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
                    break;
                case ErrorResponse er:
                    _output.WriteLine(JsonSerializer.Serialize(er, new JsonSerializerOptions { WriteIndented = true }));
                    break;
            }
            sql.SendQuery(query);
            response = sql.ReadQueryResult(TimeSpan.FromSeconds(30));
            Assert.Equal(45, receivedCount);
		}
	}
	public class DataOp
    {
		public string Text { get; set; }
    }
}
