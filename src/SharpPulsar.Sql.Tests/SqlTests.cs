using System;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Sql.Public;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Sql.Tests
{
    
    public class SqlTests
    {
        private ActorSystem _actorSystem;
        private readonly ITestOutputHelper _output;
        public SqlTests(ITestOutputHelper output) 
        {
            _output = output;
            _actorSystem = ActorSystem.Create("Sql"); 
        }
        [Fact]
        public async Task Sql_With_Excute_Set()
        {
            var topic = "query_topics_avro";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
            var sql = new SqlInstance(_actorSystem, option);
            var data = await sql.ExecuteAsync();
            Assert.NotNull(data);
            IQueryResponse resp = null;
            switch (data.Response)
            {
                case StatsResponse stats:
                    resp = stats;
                    break;
                case DataResponse dt:
                    resp = dt;
                    break;
                case ErrorResponse er:
                    resp = er;
                    break;
            }
            _output.WriteLine(JsonSerializer.Serialize(resp, new JsonSerializerOptions { WriteIndented = true }));
        }
        [Fact]
        public async Task Sql_With_Query_Set()
        {
            var topic = "query_topics_avro";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Catalog = "pulsar", Schema = "public/default" };
            var sql = new SqlInstance(_actorSystem, option);
            var data = await sql.ExecuteAsync(query:@$"select * from ""{topic}""");
            Assert.NotNull(data);
            IQueryResponse resp = null;
            switch (data.Response)
            {
                case StatsResponse stats:
                    resp = stats;
                    break;
                case DataResponse dt:
                    resp = dt;
                    break;
                case ErrorResponse er:
                    resp = er;
                    break;
            }
            _output.WriteLine(JsonSerializer.Serialize(resp, new JsonSerializerOptions { WriteIndented = true }));
        }

        [Fact]
        public async Task Live_Sql()
        {
            var topic = "query_topics_avro";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}"" where __publish_time__ > {{time}}", Catalog = "pulsar", Schema = "public/default" };
            var sql = new LiveSqlInstance(_actorSystem, option, topic, TimeSpan.FromMilliseconds(5000), DateTime.Parse("1970-01-18 20:27:56.387"));
            await Task.Delay(TimeSpan.FromSeconds(10));
            await foreach (var data in sql.ExecuteAsync())
            {
                Assert.NotNull(data);
                IQueryResponse resp = null;
                switch (data.Response)
                {
                    case StatsResponse stats:
                        resp = stats;
                        break;
                    case DataResponse dt:
                        resp = dt;
                        break;
                    case ErrorResponse er:
                        resp = er;
                        break;
                }
                _output.WriteLine(JsonSerializer.Serialize(resp, new JsonSerializerOptions { WriteIndented = true }));
            }

        }
    }
}
