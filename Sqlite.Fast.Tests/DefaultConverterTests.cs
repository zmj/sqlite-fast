using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class DefaultConverterTests
    {
        [Fact]
        public void Bool_Integer_True()
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
        public void Bool_Integer_False()
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
        public void BoolNull_Integer_True()
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
        public void BoolNull_Integer_False()
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
        public void DateTimeOffset_Integer_Value()
        {
            P<DateTimeOffset> p = default;
            R<DateTimeOffset> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                var dt = DateTimeOffset.Now;
                p.Value = dt;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Integer_Value()
        {
            P<DateTimeOffset?> p = default;
            R<DateTimeOffset?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                var dt = DateTimeOffset.Now;
                p.Value = dt;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(dt, r.Value);
            }
        }

        [Fact]
        public void DateTimeOffsetNull_Null()
        {
            P<DateTimeOffset?> p = default;
            R<DateTimeOffset?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }

        [Theory]
        [InlineData("D")]
        [InlineData("B")]
        [InlineData("P")]
        [InlineData("N")]
        public void Guid_Text(string format)
        {
            P<string> p = default;
            R<Guid> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                Guid g = Guid.NewGuid();
                p.Value = g.ToString(format);
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(g, r.Value);
            }
        }

        [Fact]
        public void Guid_Text_Invalid()
        {
            P<string> p = default;
            R<Guid> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = "not a guid";
                insert.Bind(p).Execute();
                Assert.Throws<AssignmentException>(() => select.Execute(ref r));
            }
        }

        [Fact]
        public void GuidNull_Text_Value()
        {
            P<string> p = default;
            R<Guid?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                Guid g = Guid.NewGuid();
                p.Value = g.ToString();
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(g, r.Value);
            }
        }

        [Fact]
        public void GuidNull_Text_Null()
        {
            P<string> p = default;
            R<Guid?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }

        [Fact]
        public void TimeSpan_Integer_Value()
        {
            P<TimeSpan> p = default;
            R<TimeSpan> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                p.Value = t;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void Timespan_Text_Value()
        {
            P<string> p = default;
            R<TimeSpan> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                p.Value = t.ToString();
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }
        
        [Fact]
        public void TimeSpanNull_Integer_Value()
        {
            P<TimeSpan?> p = default;
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                p.Value = t;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void TimeSpanNull_Text_Value()
        {
            P<string> p = default;
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                p.Value = t.ToString();
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void TimeSpanNull_Null()
        {
            P<TimeSpan?> p = default;
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData(null)]
        public void String(string value)
        {
            P<string> p = default;
            R<string> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(value, r.Value);
            }
        }
    }
}
