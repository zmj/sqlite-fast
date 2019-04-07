using System;
using Xunit;


namespace Sqlite.Fast.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void ValueTuple()
        {
            using (var tbl = new TestTable("create table t (x int, y text)"))
            using (var insert = tbl.Stmt<(int, string)>("insert into t values (@x, @y)"))
            using (var select = tbl.RStmt<(int, string)>("select x,y from t"))
            {
                var value = (1, "one");
                insert.Bind(value).Execute();
                (int, string) r = default;
                Assert.True(select.Execute(ref r));
                Assert.Equal(value, r);
            }
        }

        [Fact]
        public void Custom_ToInt()
        {
            P<int> p = default;
            var conv = ParameterConverter.Builder<P<int>>()
                .With(pa => pa.Value, v => -1 * v)
                .Ignore(pa => pa.C).Compile();
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", conv))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = 5;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(-1 * p.Value, r.Value);
            }
        }

        [Fact]
        public void Custom_FromInt()
        {
            P<int> p = default;
            R<int> r = default;
            var conv = ResultConverter.Builder<R<int>>()
                .With(re => re.Value, (long v) => -1 * (int)v).Compile();
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", conv))
            {
                p.Value = 5;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(-1 * p.Value, r.Value);
            }
        }

        [Fact]
        public void Scalar_FromInt()
        {
            R<int> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt<int>("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                int value = 5;
                insert.Bind(value).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(value, r.Value);
            }
        }

        [Fact]
        public void Scalar_ToInt()
        {
            P<int> p = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.RStmt<int>("select x from t"))
            {
                p.Value = 5;
                insert.Bind(p).Execute();
                int r = default;
                Assert.True(select.Execute(ref r));
                Assert.Equal(p.Value, r);
            }
        }

        [Fact]
        public void Scalar_FromString()
        {
            R<string> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt<string>("insert into t values (@x)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                string value = "hello";
                insert.Bind(value).Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(value, r.Value);
            }
        }

        [Fact]
        public void Scalar_ToString()
        {
            P<string> p = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.RStmt<string>("select x from t"))
            {
                p.Value = "hello";
                insert.Bind(p).Execute();
                string r = null!;
                Assert.True(select.Execute(ref r));
                Assert.Equal(p.Value, r);
            }
        }

        private struct Struct { }

        [Fact]
        public void From_Missing()
        {
            P<Struct> p = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            {                
                Assert.Throws<BindingException>(() => insert.Bind(p));
            }
        }

        private static T ThrowConvertTo<T>() => throw new Exception($"throw: {typeof(T).Name}");

        [Fact]
        public void From_Fails()
        {
            P<int> p = default;
            var conv = ParameterConverter.Builder<P<int>>()
                .With(pa => pa.Value, v => ThrowConvertTo<long>())
                .Ignore(pa => pa.C)
                .Compile();
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", conv))
            {
                Assert.Throws<BindingException>(() => insert.Bind(p));
            }
        }

        [Fact]
        public void To_Missing()
        {
            R<Struct> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.Throws<AssignmentException>(() => select.Execute(ref r));
            }
        }

        [Fact]
        public void To_Fails()
        {
            R<int> r = default;
            var conv = ResultConverter.Builder<R<int>>()
                .With(re => re.Value, (long v) => throw new Exception("throw"))
                .Compile();
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values(1)"))
            using (var select = tbl.Stmt("select x from t", conv))
            {
                insert.Execute();
                Assert.Throws<AssignmentException>(() => select.Execute(ref r));
            }
        }

        [Fact]
        public void Result_Ignore()
        {
            R<string, int> r = default;
            var conv = ResultConverter.Builder<R<string, int>>()
                .Ignore(re => re.Value1)
                .Compile();
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (1)"))
            using (var select = tbl.Stmt("select x from t", conv))
            {
                insert.Execute();
                Assert.True(select.Execute(ref r));
                Assert.Equal(1, r.Value2);
            }
        }

        [Fact]
        public void Custom_ScalarGuidToText()
        {
            var conv = ParameterConverter.ScalarBuilder<Guid>()
                .With((g, b) => g.ToString().AsSpan().CopyTo(b), _ => 36)
                .Compile();
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", conv))
            using (var select = tbl.RStmt<Guid>("select x from t"))
            {
                Guid g = Guid.NewGuid();
                insert.Bind(g).Execute();
                Guid r = default;
                select.Execute(ref r);
                Assert.Equal(g, r);
            }
        }

        [Fact]
        public void Custom_ToBlob()
        {
            var pc = ParameterConverter.ScalarBuilder<int>()
                .With((int i, Span<byte> span) => span[0] = (byte)(i + 1), _ => 1)
                .Compile();
            var rc = ResultConverter.ScalarBuilder<int>()
                .With((ReadOnlySpan<byte> span) => span[0] + 1)
                .Compile();
            using (var tbl = new TestTable("create table t (x)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", pc))
            using (var select = tbl.RStmt("select x from t", rc))
            {
                insert.Bind(1).Execute();
                int i = 0;
                Assert.True(select.Execute(ref i));
                Assert.Equal(3, i);
            }
        }
    }
}
