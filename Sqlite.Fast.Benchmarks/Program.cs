using System;
using BenchmarkDotNet.Running;
using Sqlite.Fast.Benchmarks;

BenchmarkRunner.Run<AssignmentBenchmark>();

/* Profile();

static void Profile()
{
    var b = new AssignmentBenchmark();
    b.Setup();
    b.Assign();
    b.Cleanup();
}*/