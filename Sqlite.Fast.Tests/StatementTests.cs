using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class StatementTests
    {
        [Fact]
        public void Statement0_Execute_Disposed()
        {
            using (var conn = new Connection())
            {
                var stmt = conn.CompileStatement("select 1");
                stmt.Dispose();
                Assert.Throws<ObjectDisposedException>(
                    () => stmt.Execute());
            }
        }

        [Fact]
        public void Statement0_DisposeTwice()
        {
            using (var conn = new Connection())
            {
                var stmt = conn.CompileStatement("select 1");
                stmt.Dispose();
                stmt.Dispose();
            }
        }

        [Fact]
        public void Statement1_Bind_ParamsNull()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var stmt = tbl.Stmt<string?>("insert into t values (@x)"))
            {
                stmt.Bind(null);
            }
        }

        [Fact]
        public void Statement1_Bind_ConverterNull()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var stmt = tbl.Stmt<int>("insert into t values (@x)"))
            {
                ParameterConverter<string> p = null!;
                Assert.Throws<ArgumentNullException>(
                    () => stmt.Bind(p, ""));
            }
        }

        [Fact]
        public void Statement1_Bind_Disposed()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            {
                var stmt = tbl.Stmt<int>("insert into t values (@x)");
                stmt.Dispose();
                Assert.Throws<ObjectDisposedException>(
                    () => stmt.Bind(1));
            }
        }

        [Fact]
        public void Statement1R_Execute_ResultNull()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.RStmt<string?>("select x from t"))
            {
                insert.Execute();
                string? s = null;
                Assert.True(select.Execute(out s));
                Assert.Null(s);
            }
        }

        [Fact]
        public void Statement1R_Execute_ConverterNull()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var select = tbl.RStmt<int>("select x from t"))
            {
                ResultConverter<string> r = null!;
                string? s = null;
                Assert.Throws<ArgumentNullException>(
                    () => select.Execute(r, out s));
            }
        }

        [Fact]
        public void Statement1R_Execute_Disposed()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            {
                var select = tbl.RStmt<string?>("select x from t");
                select.Dispose();
                string? s = null;
                Assert.Throws<ObjectDisposedException>(
                    () => select.Execute(out s));
            }
        }

        [Fact]
        public void Statement2_Bind_ParamNull()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var stmt = tbl.Stmt<string, string?>("select x from t where x=@x"))
            {
                stmt.Bind(null);
            }
        }

        [Fact]
        public void Statement2_Bind_ConverterNull()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var stmt = tbl.Stmt<int, int>("select x from t where x=@x"))
            {
                ParameterConverter<string> p = null!;
                Assert.Throws<ArgumentNullException>(
                    () => stmt.Bind(p, ""));
            }
        }

        [Fact]
        public void Statement2_Execute_ResultNull()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values ('x')"))
            using (var stmt = tbl.Stmt<string?, string?>("select null from t where x=@x"))
            {
                insert.Execute();
                string? s = null;
                stmt.Bind("x");
                Assert.True(stmt.Execute(out s));
                Assert.Null(s);
            }
        }

        [Fact]
        public void Statement2_Execute_ConveterNull()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var stmt = tbl.Stmt<int, int>("select x from t where x=@x"))
            {
                ResultConverter<string> r = null!;
                string? s = null;
                Assert.Throws<ArgumentNullException>(
                    () => stmt.Execute(r, out s));
            }
        }

        [Fact]
        public void Statement2_Bind_Disposed()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            {
                var stmt = tbl.Stmt<int, int>("select x from t where x=@x");
                stmt.Dispose();
                Assert.Throws<ObjectDisposedException>(
                    () => stmt.Bind(1));
            }
        }

        [Fact]
        public void Statement2_Execute_Disposed()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            {
                var stmt = tbl.Stmt<int, int>("select x from t where x=@x");
                stmt.Dispose();
                int x = default;
                Assert.Throws<ObjectDisposedException>(
                    () => stmt.Execute(out x));
            }
        }

        [Fact]
        public void Statement2_DisposedTwice()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            {
                var stmt = tbl.Stmt<int, int>("select x from t where x=@x");
                stmt.Dispose();
                stmt.Dispose();
            }
        }
    }
}
