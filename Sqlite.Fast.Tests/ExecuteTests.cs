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
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(1)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<int> r = default;
                select.Execute(r.Map, ref r);
                Assert.Equal(1, r.Value);
            }
        }

        [Fact]
        public void Rebind() 
        {
            using (var tbl = new TestTable("create table t (x int)")) 
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var sum = tbl.Stmt("select sum(x) from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                sum.Execute(r.Map, ref r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void SelectMany() 
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                R<int> r = default;
                foreach (var row in select.Execute(r.Map))
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
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                int sum = 0;
                R<int> r = default;
                var rows = select.Execute(r.Map);
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
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t where x=@x"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                var enumerator = select.Bind(0, 1).Execute(r.Map).GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(1, r.Value);

                select.Bind(0, 2).Execute(r.Map, ref r);
                Assert.Equal(2, r.Value);
            }
        }
        
        [Fact]
        public void Rebind_Enumerate_Throws()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select x from t where x=@x"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                var enumerator = select.Bind(0, 1).Execute(r.Map).GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(1, r.Value);
                select.Bind(0, 2);
                Assert.Throws<Exception>(() => enumerator.MoveNext());
            }
        }

        [Fact]
        public void Enumerate_Reexecute()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select sum(x) from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                var enumerator = select.Execute(r.Map).GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(3, r.Value);
                r = default;
                select.Execute(r.Map, ref r);
                Assert.Equal(3, r.Value);
            }
        }

        [Fact]
        public void Reexecute_Enumerate_Throws()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(@x)"))
            using (var select = tbl.Stmt("select sum(x) from t"))
            {
                insert.Bind(0, 1).Execute();
                insert.Bind(0, 2).Execute();
                R<int> r = default;
                var enumerator = select.Execute(r.Map).GetEnumerator();
                Assert.True(enumerator.MoveNext());
                enumerator.Current.AssignTo(ref r);
                Assert.Equal(3, r.Value);
                select.Execute(r.Map, ref r);
                Assert.Throws<Exception>(() => enumerator.MoveNext());
            }
        }
    }
}
