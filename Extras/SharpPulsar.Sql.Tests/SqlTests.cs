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
        [Fact(Skip = "")]
        public async Task Sql_With_Excute_Set()
        {
            var topic = "keyvalue";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
            var sql = new SqlInstance(_actorSystem, option);
            var data = await sql.ExecuteAsync();
            Assert.NotNull(data);
            _output.WriteLine(JsonSerializer.Serialize((DataResponse)data.Response, new JsonSerializerOptions{WriteIndented = true}));
        }
        [Fact(Skip = "")]
        public async Task Sql_With_Query_Set()
        {
            var topic = "keyvalue";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Catalog = "pulsar", Schema = "public/default" };
            var sql = new SqlInstance(_actorSystem, option);
            var data = await sql.ExecuteAsync(query:@$"select * from ""{topic}""");
            Assert.NotNull(data);
            _output.WriteLine(JsonSerializer.Serialize((DataResponse)data.Response, new JsonSerializerOptions { WriteIndented = true }));
        }

        [Fact(Skip = "")]
        public async Task Live_Sql()
        {
            var topic = "keyvalue";
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}"" where __publish_time__ > {{time}}", Catalog = "pulsar", Schema = "public/default" };
            var sql = new LiveSqlInstance(_actorSystem, option, topic, TimeSpan.FromMilliseconds(5000), DateTime.Parse("1970-01-18 20:27:56.387"));
            await Task.Delay(TimeSpan.FromSeconds(10));
            await foreach (var data in sql.ExecuteAsync())
            {
                Assert.NotNull(data);
                _output.WriteLine(JsonSerializer.Serialize((DataResponse)data.Response, new JsonSerializerOptions { WriteIndented = true }));
            }

        }
    }
}
