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

        [Fact]
        public void SelectOne()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(1)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                select.Execute(ref r);
                Assert.Equal(1, r.Value);
            }
        }

        [Fact]
        public void Rebind()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var sum = tbl.Stmt("select sum(x) from t", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                sum.Execute(ref r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void SelectMany()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                foreach (var row in select.Execute())
                {
                    row.AssignTo(ref r);
                    sum += r.Value;
                }
                Assert.Equal(3, sum);
            }
        }

        [Fact]
        public void SelectMany_Twice()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                var rows = select.Execute();
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

        [Fact]
        public void Enumerate_Rebind()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t where x=@x", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                var enumerator = select.Bind(0, 1).Execute().GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(1, r.Value);

                select.Bind(0, 2).Execute(ref r);
                Assert.Equal(2, r.Value);
            }
        }
        
        [Fact]
        public void Rebind_Enumerate_Throws()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t where x=@x", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                var enumerator = select.Bind(0, 1).Execute().GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(1, r.Value);
                select.Bind(0, 2);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }
        }

        [Fact]
        public void Enumerate_Reexecute()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select sum(x) from t", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                var enumerator = select.Execute().GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(3, r.Value);
                r = default;
                select.Execute(ref r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void Reexecute_Enumerate_Throws()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select sum(x) from t", r.C))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                var enumerator = select.Execute().GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(3, r.Value);
                select.Execute(ref r);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }
        }
    }
}
