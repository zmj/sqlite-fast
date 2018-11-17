using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void Bool_True()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<bool> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.True(r.Value);
            }
        }

        [Fact]
        public void Bool_False()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (0)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<bool> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.False(r.Value);
            }
        }

        [Fact]
        public void BoolNull_True()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<bool?> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.True(r.Value);
            }
        }


        [Fact]
        public void BoolNull_False()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (0)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<bool?> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.False(r.Value);
            }
        }
        
        [Fact]
        public void BoolNull_Null()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<bool?> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.Null(r.Value);
            }
        }

        [Fact]
        public void DateTimeOffset_Value()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                var dt = DateTimeOffset.Now;
                insert.Bind(0, dt.UtcTicks).Execute();
                R<DateTimeOffset> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Value()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                var dt = DateTimeOffset.Now;
                insert.Bind(0, dt.UtcTicks).Execute();
                R<DateTimeOffset?> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Null()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t"))
            {
                insert.Execute();
                R<DateTimeOffset?> r = default;
                Assert.True(select.Execute(r.Map, ref r));
                Assert.Null(r.Value);
            }
        }
    }
}
