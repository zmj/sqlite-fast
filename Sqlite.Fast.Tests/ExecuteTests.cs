using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void SelectOne()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(1)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                select.Execute(out r);
                Assert.Equal(1, r.Value);
            }
        }

        [Fact]
        public void Rebind()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var sum = tbl.Stmt("select sum(x) from t", r.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                sum.Execute(out r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void SelectMany()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                int sum = 0;
                foreach (var row in select.Execute())
                {
                    row.AssignTo(out r);
                    sum += r.Value;
                }
                Assert.Equal(3, sum);
            }
        }

        [Fact]
        public void SelectMany_Twice()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                int sum = 0;
                var rows = select.Execute();
                foreach (var row in rows)
                {
                    row.AssignTo(out r);
                    sum += r.Value;
                }
                foreach (var row in rows) 
                {
                    row.AssignTo(out r);
                    sum += r.Value;
                }
                Assert.Equal(6, sum);
            }
        }

        [Fact]
        public void Disposed_Bind()
        {
            using (var tbl = new TestTable("create table t (x)"))
            {
                var insert = tbl.Stmt<int>("insert into t values (@x)");
                insert.Dispose();
                Assert.Throws<ObjectDisposedException>(() => insert.Bind(1));
            }
        }

        [Fact]
        public void Disposed_Execute()
        {
            using (var tbl = new TestTable("create table t (x)"))
            {
                var select = tbl.RStmt<int>("select x from t");
                select.Dispose();
                Assert.Throws<ObjectDisposedException>(() => select.Execute());
            }
        }

        [Fact]
        public void Enumerate_Rebind()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var select = tbl.Stmt("select x from t where x=@x", r.C, p.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                p.Value = 1;
                using (var enumerator = select.Bind(p).Execute().GetEnumerator())
                {
                    Assert.True(enumerator.MoveNext());
                    enumerator.Current.AssignTo(out r);
                    Assert.Equal(1, r.Value);
                }
                p.Value = 2;
                select.Bind(p).Execute(out r);
                Assert.Equal(2, r.Value);
            }
        }

        [Fact]
        public void Enumerate_Reset()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var select = tbl.Stmt("select x from t where x=@x", r.C, p.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                p.Value = 1;
                using (var enumerator = select.Bind(p).Execute().GetEnumerator())
                {
                    Assert.True(enumerator.MoveNext());
                    enumerator.Current.AssignTo(out r);
                    Assert.Equal(1, r.Value);

                    enumerator.Reset();
                    Assert.True(enumerator.MoveNext());
                    enumerator.Current.AssignTo(out r);
                    Assert.Equal(1, r.Value);
                }
            }
        }
        
        [Fact]
        public void Enumerate_Reexecute()
        {
            P<int> p = default;
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)", p.C))
            using (var select = tbl.Stmt("select sum(x) from t", r.C))
            {
                p.Value = 1;
                insert.Bind(p).Execute();
                p.Value = 2;
                insert.Bind(p).Execute();
                using (var enumerator = select.Execute().GetEnumerator())
                {
                    Assert.True(enumerator.MoveNext());
                    enumerator.Current.AssignTo(out r);
                    Assert.Equal(3, r.Value);
                }
                r = default;
                select.Execute(out r);
                Assert.Equal(3, r.Value);
            }
        }
        
        [Fact]
        public void Execute_Bind_Multiple()
        {
            P<int, string> p = default;
            R<int, string> r = default;
            using (var tbl = new TestTable("create table t (x int, y text)"))
            using (var insert = tbl.Stmt("insert into t values (@x, @y)", p.C))
            using (var select = tbl.Stmt("select x, y from t", r.C))
            {
                p.Value1 = 1;
                p.Value2 = "one";
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(1, r.Value1);
                Assert.Equal("one", r.Value2);
            }
        }

        [Fact]
        public void Execute_Bind_Rebind()
        {
            P<int, string> p = default;
            R<int, string> r = default;
            using (var tbl = new TestTable("create table t (x int, y text)"))
            using (var insert = tbl.Stmt("insert into t values (@x, @y)", p.C))
            using (var select = tbl.Stmt("select x, y from t", r.C))
            {
                p.Value1 = 1;
                p.Value2 = "1";
                insert.Bind(p).Execute();
                p.Value1 = 2;
                p.Value2 = "2";
                insert.Bind(p).Execute();
                int sum = 0;
                foreach (var row in select.Execute())
                {
                    row.AssignTo(out r);
                    sum += r.Value1;
                    Assert.Equal(r.Value1.ToString(), r.Value2);
                }
                Assert.Equal(3, sum);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void SelectMany_Array(int n)
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt<int>("insert into t values (@x)"))
            using (var select = tbl.RStmt<int>("select x from t"))
            {
                int expectedSum = 0;
                for (int i = 0; i < n; i++)
                {
                    insert.Bind(i).Execute();
                    expectedSum += i;
                }
                int j = 0;
                int[] values = new int[n];
                foreach (Row<int> row in select.Execute())
                {
                    row.AssignTo(out values[j++]);
                }
                Assert.Equal(expectedSum, Enumerable.Sum(values));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void SelectMany_Linq(int n)
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt<int>("insert into t values (@x)"))
            using (var select = tbl.RStmt<int>("select x from t"))
            {
                int expectedSum = 0;
                for (int i = 0; i < n; i++)
                {
                    insert.Bind(i).Execute();
                    expectedSum += i;
                }
                IEnumerable<Row<int>> rows = select.Execute();
                int sum = rows.Sum(r => { r.AssignTo(out int x); return x; });
                Assert.Equal(expectedSum, sum);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void SelectMany_IEnumerable(int n)
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt<int>("insert into t values (@x)"))
            using (var select = tbl.RStmt<int>("select x from t"))
            {
                int expectedSum = 0;
                for (int i = 0; i < n; i++)
                {
                    insert.Bind(i).Execute();
                    expectedSum += i;
                }
                System.Collections.IEnumerable rows = select.Execute();
                int sum = 0;
                foreach (object obj in rows)
                {
                    ((Row<int>)obj).AssignTo(out int x);
                    sum += x;
                }
                Assert.Equal(expectedSum, sum);
            }
        }
    }
}
