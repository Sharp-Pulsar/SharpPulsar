using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SharpPulsar.Benchmarks.Bench;

namespace SharpPulsar.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmarks = Assembly.GetExecutingAssembly().GetTypes()
               .Where(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
               .Any(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Any()))
               .OrderBy(t => t.Namespace)
               .ThenBy(t => t.Name)
               .ToArray();
               var benchmarkSwitcher = new BenchmarkSwitcher(benchmarks);
               benchmarkSwitcher.Run(args);
        }
    }
}
