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
        public void DateTimeOffsetNull_Integer_Value()
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

        [Theory]
        [InlineData("D")]
        [InlineData("B")]
        [InlineData("P")]
        [InlineData("N")]
        public void Guid_Text(string format)
        {
            R<Guid> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                Guid g = Guid.NewGuid();
                insert.Bind(0, g.ToString(format)).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(g, r.Value);
            }
        }

        [Fact]
        public void Guid_Text_Invalid()
        {
            R<Guid> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(0, "not a guid").Execute();
                Assert.Throws<AssignmentException>(() => select.Execute(ref r));
            }
        }

        [Fact]
        public void GuidNull_Text_Value()
        {
            R<Guid?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                Guid g = Guid.NewGuid();
                insert.Bind(0, g.ToString()).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(g, r.Value);
            }
        }

        [Fact]
        public void GuidNull_Text_Null()
        {
            R<Guid?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.Null(r.Value);
            }
        }

        [Fact]
        public void TimeSpan_Integer_Value()
        {
            R<TimeSpan> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                insert.Bind(0, t.Ticks).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void Timespan_Text_Value()
        {
            R<TimeSpan> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                insert.Bind(0, t.ToString()).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }
        
        [Fact]
        public void TimeSpanNull_Integer_Value()
        {
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                insert.Bind(0, t.Ticks).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void TimeSpanNull_Text_Value()
        {
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                TimeSpan t = TimeSpan.FromMinutes(42);
                insert.Bind(0, t.ToString()).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(t, r.Value);
            }
        }

        [Fact]
        public void TimeSpanNull_Null()
        {
            R<TimeSpan?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(0, (string)null).Execute();
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
            R<string> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Bind(0, value).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(value, r.Value);
            }
        }
    }
}
