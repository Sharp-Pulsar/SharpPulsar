using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace SharpPulsar.Benchmarks
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(StatisticColumn.AllStatistics);
        }
    }
}
