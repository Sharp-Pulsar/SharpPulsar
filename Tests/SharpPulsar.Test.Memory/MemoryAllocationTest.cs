
using System;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleTables;
using JetBrains.dotMemoryUnit;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Memory.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Memory
{
    [Collection(nameof(MemoryAllocation))]
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    public class MemoryAllocationTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        //dotMemoryUnit.exe "c:\Program Files\dotnet\dotnet.exe" -- test "C:\Users\User\source\repos\SharpPulsar\Tests\SharpPulsar.Test.Memory\bin\Release\net5.0\SharpPulsar.Test.Memory.dll"
        public MemoryAllocationTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
        }
        [Fact]
        public void Produce_Consumer_Allocation()
        {
            var topic = $"memory-allocation-{Guid.NewGuid()}";
            var consumer = _client.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(topic));


            for (var i = 0; i < 50; i++)
            {
                var data = Encoding.UTF8.GetBytes($"memory-allocation-{i}");
                producer.NewMessage().Value(data).Send();
            }
            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 50; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
                }
            }
            dotMemory.Check(memory =>
            {
                var result = memory
                    .GetObjects(where => @where.Namespace.Like("SharpPulsar.*"));

                _output.WriteLine(
                    ConsoleTable.From(
                        result
                        .GroupByType()
                        .Select(t => new
                        {
                            t.Type.Name,
                            t.ObjectsCount,
                            t.SizeInBytes
                        })
                    ).ToMarkDownString());

                Assert.True(result.ObjectsCount > 0);
            });
        }
    }    
}