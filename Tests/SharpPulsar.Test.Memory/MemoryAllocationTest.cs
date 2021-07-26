
using System;
using System.Linq;
using System.Text;
using ConsoleTables;
using JetBrains.dotMemoryUnit;
using JetBrains.dotMemoryUnit.Client.Interface;
using JetBrains.dotMemoryUnit.Properties;
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

        public MemoryAllocationTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
        }
        [Fact]
        public void Produce_Consumer_Allocation()
        {
            var producer = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic("memory-allocation"));


            for (var i = 0; i < 50; i++)
            {
                var data = Encoding.UTF8.GetBytes($"memory-allocation-{i}");
                producer.NewMessage().Value(data).Send();
            }

            var consumer = _client.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic("memory-allocation")
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

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
                    .GetObjects(where =>
                        where.Type.IsInNamespace<Producer<byte[]>>());

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

                //Assert.Equal(3, result.ObjectsCount);
            });
        }
    }
    public static class TypePropertyExtensions
    {
        public static Query IsInNamespace<TType>(this TypeProperty where)
        {
            var type = typeof(TType);
            var types = type.Assembly.GetTypes()
                .Where(t => t.Namespace?.StartsWith("SharpPulsar") == true)
                .ToArray();

            return where.Is(types);
        }
    }
}
