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
        private ResultStatement<Record>? _select;

        private struct Record
        {
            public Guid PrimaryKey;
            public Guid ForeignKey;
            public int NumberOne;
            public int NumberTwo;
            public DateTimeOffset DateOne;
            public DateTimeOffset DateTwo;
        }

        [GlobalSetup]
        public void Setup()
        {
            _connection = new Connection();
            using (var tbl = _connection.CompileStatement(
                "create table t (id text primarykey, id2 text, n int, n2 int, time int, time2 int)"))
            {
                tbl.Execute(); 
            }
            using (var insert = _connection.CompileStatement<Record>(
                "insert into t values (@id, @id2, @n, @n2, @time, @time2)"))
            {
                for (int i = 0; i < N; i++)
                {
                    insert.Bind(new Record 
                    {
                        PrimaryKey = Guid.NewGuid(),
                        ForeignKey = Guid.NewGuid(),
                        NumberOne = i,
                        NumberTwo = ~i,
                        DateOne = DateTimeOffset.UtcNow,
                        DateTwo = DateTimeOffset.Now,
                    }).Execute();
                }
            }
            _select = _connection.CompileResultStatement<Record>(
                    "select id, id2, n, n2, time, time2 from t");
        }

        [GlobalCleanup]
        public void Cleanup() 
        {
            _select?.Dispose();
            _connection?.Dispose();
        }

        [Benchmark]
        public void Assign()
        {
            Record z;
            foreach (var row in _select!.Execute()) 
            { 
                row.AssignTo(out z); 
            }
        }
    }
}