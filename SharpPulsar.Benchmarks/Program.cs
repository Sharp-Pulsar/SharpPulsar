using System;
using BenchmarkDotNet.Running;
using SharpPulsar.Benchmarks.Bench;

namespace SharpPulsar.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ProduceAndConsume>();
            Console.ReadLine();
        }
    }
}
