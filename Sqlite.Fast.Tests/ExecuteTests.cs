using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class ExecuteTests
    {
        [Fact]
        public void CreateTable()
        {
            using (new TestTable("create table t (x int)"))
            {
            }
        }

        private struct R<T> 
        { 
            public T Value; 
            public static readonly RowToRecordMap<R<T>> Map 
                = RowToRecordMap.Default<R<T>>().Compile();
        }

        [Fact]
        public void Execute_SelectOne() 
        {
            using (var tbl = new TestTable("create table t (x int")) 
            using (var insert = tbl.Stmt("insert into t values(1)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<int> r = default;
                select.Execute(R<int>.Map, ref r);
                Assert.Equal(1, r.Value);
            }
        }

        [Fact]
        public void Execute_Rebind() 
        {
            using (var tbl = new TestTable("create table t (x int")) 
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var sum = tbl.Stmt("select sum(x) from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                sum.Execute(R<int>.Map, ref r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void Execute_SelectMany() 
        {
            using (var tbl = new TestTable("create table t (x int"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                R<int> r = default;
                foreach (var row in select.Execute(R<int>.Map))
                {
                    row.AssignTo(ref r);
                    sum += r.Value;
                }
                Assert.Equal(3, sum);
            }
        }

        [Fact]
        public void Execute_SelectMany_Twice() 
        {
            using (var tbl = new TestTable("create table t (x int"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                var rows = select.Execute(R<int>.Map);
                R<int> r = default;
                foreach (var row in rows)
                {
                    row.AssignTo(ref r);
                    sum += r.Value;
                }
                foreach (var row in rows) 
                {
                    row.AssignTo(ref r);
                    sum += r.Value;
                }
                Assert.Equal(6, sum);
            }
        }
    }
}
