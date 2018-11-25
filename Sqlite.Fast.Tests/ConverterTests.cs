using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void Bool_True()
        {
            R<bool> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.True(r.Value);
            }
        }

        [Fact]
        public void Bool_False()
        {
            R<bool> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (0)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.False(r.Value);
            }
        }

        [Fact]
        public void BoolNull_True()
        {
            R<bool?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.True(r.Value);
            }
        }


        [Fact]
        public void BoolNull_False()
        {
            R<bool?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (0)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.False(r.Value);
            }
        }
        
        [Fact]
        public void BoolNull_Null()
        {
            R<bool?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }

        [Fact]
        public void DateTimeOffset_Value()
        {
            R<DateTimeOffset> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                var dt = DateTimeOffset.Now;
                insert.Bind(0, dt.UtcTicks).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Value()
        {
            R<DateTimeOffset?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                var dt = DateTimeOffset.Now;
                insert.Bind(0, dt.UtcTicks).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Null()
        {
            R<DateTimeOffset?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }
    }
}
