using System;
using BenchmarkDotNet.Attributes;
using Sqlite.Fast;

namespace Sqlite.Fast.Benchmarks
{
    [MemoryDiagnoser]
    public class AssignmentBenchmark
    {
        private const int N = 1_000_000;
        private Connection? _connection;
        private Random? _random;

        [GlobalSetup]
        public void Setup()
        {
            _connection = new Connection();
            _random = new Random(N);
            using (var tbl = _connection.CompileStatement(
                "create table t (id text primarykey, n int, time int)"))
            {
                tbl.Execute(); 
            }
            using (var insert = _connection.CompileStatement<(Guid, int, DateTimeOffset)>(
                "insert into t values (@id, @n, @time)"))
            {
                for (int i = 0; i < N; i++)
                {
                    insert.Bind((Guid.NewGuid(), i, DateTimeOffset.UtcNow)).Execute();
                }
            }
        }

        [GlobalCleanup]
        public void Cleanup() => _connection?.Dispose();

        [Benchmark]
        public void Assign()
        {
            using (var select = _connection!
                .CompileResultStatement<(Guid, int, DateTimeOffset)>(
                    "select id, n, time from t"))
            {
                (Guid, int, DateTimeOffset) z;
                foreach (var row in select.Execute()) { row.AssignTo(out z); }
            }
        }
    }
}