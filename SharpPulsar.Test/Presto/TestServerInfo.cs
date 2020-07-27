using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SharpPulsar.Presto;
using Xunit;

namespace SharpPulsar.Test.Presto
{
    public class TestServerInfo
    {
        [Fact]
        public void TestJsonRoundTrip()
        {
            AssertJsonRoundTrip(new ServerInfo{NodeVersion = new NodeVersion{Version = "Unknown"}, Environment = "test", Coordinator = true, Starting = false, Uptime = TimeSpan.FromMinutes(2)});
            AssertJsonRoundTrip(new ServerInfo{NodeVersion = new NodeVersion{Version = "Unknown"}, Environment = "test", Coordinator = true, Starting = false, Uptime = TimeSpan.Zero});
           
        }
        [Fact]
        public void TestBackwardsCompatible()
        {
            var newServerInfo = new ServerInfo { NodeVersion = new NodeVersion { Version = "Unknown" }, Environment = "test", Coordinator = true, Starting = false, Uptime = TimeSpan.Zero };
            var legacyServerInfo = JsonSerializer.Deserialize<ServerInfo>("{\"nodeVersion\":{\"version\":\"Unknown\"},\"environment\":\"test\",\"coordinator\":true}");
            Assert.Equal(newServerInfo, legacyServerInfo);
        }

        private void AssertJsonRoundTrip(ServerInfo serverInfo)
        {
            var json = JsonSerializer.Serialize(serverInfo);
            var copy = JsonSerializer.Deserialize<ServerInfo>(json);
            Assert.Equal(copy, serverInfo);
        }
    }
}
