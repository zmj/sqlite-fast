using System;
using BenchmarkDotNet.Running;

namespace Sqlite.Fast.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AssignmentBenchmark>();
        }
    }
}
