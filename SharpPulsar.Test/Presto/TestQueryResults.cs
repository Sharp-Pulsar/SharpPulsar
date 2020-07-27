using System.Linq;
using System.Text.Json;
using SharpPulsar.Presto;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Presto
{
    public class TestQueryResults
    {
        private readonly ITestOutputHelper _output;

        public TestQueryResults(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCompatibility()
        {
            var goldenValue = "{\n" +
                                 "  \"id\" : \"20160128_214710_00012_rk68b\",\n" +
                                 "  \"infoUri\" : \"http://localhost:54855/query.html?20160128_214710_00012_rk68b\",\n" +
                                 "  \"columns\" : [ {\n" +
                                 "    \"name\" : \"_col0\",\n" +
                                 "    \"type\" : \"bigint\",\n" +
                                 "    \"typeSignature\" : {\n" +
                                 "      \"rawType\" : \"bigint\",\n" +
                                 "      \"typeArguments\" : [ ],\n" +
                                 "      \"literalArguments\" : [ ],\n" +
                                 "      \"arguments\" : [ ]\n" +
                                 "    }\n" +
                                 "  } ],\n" +
                                 "  \"data\" : [ [ 123 ] ],\n" +
                                 "  \"stats\" : {\n" +
                                 "    \"state\" : \"FINISHED\",\n" +
                                 "    \"queued\" : false,\n" +
                                 "    \"scheduled\" : false,\n" +
                                 "    \"nodes\" : 0,\n" +
                                 "    \"totalSplits\" : 0,\n" +
                                 "    \"queuedSplits\" : 0,\n" +
                                 "    \"runningSplits\" : 0,\n" +
                                 "    \"completedSplits\" : 0,\n" +
                                 "    \"cpuTimeMillis\" : 0,\n" +
                                 "    \"wallTimeMillis\" : 0,\n" +
                                 "    \"queuedTimeMillis\" : 0,\n" +
                                 "    \"elapsedTimeMillis\" : 0,\n" +
                                 "    \"processedRows\" : 0,\n" +
                                 "    \"processedBytes\" : 0,\n" +
                                 "    \"peakMemoryBytes\" : 0\n" +
                                 "  }\n" +
                                 "}";

            var results = JsonSerializer.Deserialize<QueryResults>(goldenValue);
            var dats = results.Data.ToList();
            for (var i = 0; i < dats.Count; i++)
            {
                var data = dats[i];
                foreach (var d in data)
                {
                    _output.WriteLine(d.ToString());
                }
            }
            Assert.Equal("20160128_214710_00012_rk68b", results.Id);
        }

    }
}
