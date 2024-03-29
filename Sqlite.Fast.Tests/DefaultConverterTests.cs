﻿using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class DefaultConverterTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Bool_Integer(bool value)
        {
            P<bool> p = default;
            R<bool> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void BoolNull_Integer(bool? value)
        {
            P<bool?> p = default;
            R<bool?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.Throws<AssignmentException>(() => select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
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
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData(null)]
        public void MemoryChar(string value)
        {
            P<ReadOnlyMemory<char>> p = default;
            R<ReadOnlyMemory<char>> r = default;
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value.AsMemory();
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value.AsMemory().ToString(), r.Value.ToString());
            }
        }

        private enum E { Value = 1 }
        
        [Fact]
        public void Enum()
        {
            P<E> p = default;
            R<E> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = E.Value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(E.Value, r.Value);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(null)]
        public void EnumNull(int? intValue)
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt<E?>("insert into t values (@x)"))
            using (var select = tbl.RStmt<E?>("select x from t"))
            {
                E? value = (E?)intValue;
                insert.Bind(value).Execute();
                E? r = default;
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r);
            }
        }

        private class Class { }

        [Fact]
        public void Class_Null()
        {
            R<Class> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(out r));
                Assert.Null(r.Value);
            }
        }

        private struct Struct { }

        [Fact]
        public void Struct_Null()
        {
            R<Struct?> r = default;
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (null)"))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                insert.Execute();
                Assert.True(select.Execute(out r));
                Assert.Null(r.Value);
            }
        }
        
        [Theory]
        [InlineData(3.14159)]
        [InlineData(2e-10)]
        [InlineData(0)]
        public void Double(double value)
        {
            P<double> p = default;
            R<double> r = default;
            using (var tbl = new TestTable("create table t (x float)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }
        }

        [Theory]
        [InlineData(3.14159)]
        [InlineData(2e-10)]
        [InlineData(0d)]
        [InlineData(null)]
        public void Double_Null(double? value)
        {
            P<double?> p = default;
            R<double?> r = default;
            using (var tbl = new TestTable("create table t (x float)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }
        }

        [Theory]
        [InlineData(3.14159f)]
        [InlineData(2e-10f)]
        [InlineData(0)]
        public void Float(float value)
        {
            P<float> p = default;
            R<float> r = default;
            using (var tbl = new TestTable("create table t (x float)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }

        }

        [Theory]
        [InlineData(3.14159f)]
        [InlineData(2e-10f)]
        [InlineData(0f)]
        [InlineData(null)]
        public void Float_Null(float? value)
        {
            P<float?> p = default;
            R<float?> r = default;
            using (var tbl = new TestTable("create table t (x float)"))
            using (var insert = tbl.Stmt("insert into t values (@x)", p.C))
            using (var select = tbl.Stmt("select x from t", r.C))
            {
                p.Value = value;
                insert.Bind(p).Execute();
                Assert.True(select.Execute(out r));
                Assert.Equal(value, r.Value);
            }
        }

        [Fact]
        public void Guid_Value()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt<Guid>("insert into t values (@x)"))
            using (var select = tbl.RStmt<Guid>("select x from t"))
            {
                Guid g = Guid.NewGuid();
                insert.Bind(g).Execute();
                Guid r = default;
                Assert.True(select.Execute(out r));
                Assert.Equal(g, r);
            }
        }

        [Fact]
        public void GuidNull_Value()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt<Guid?>("insert into t values (@x)"))
            using (var select = tbl.RStmt<Guid?>("select x from t"))
            {
                Guid g = Guid.NewGuid();
                insert.Bind(g).Execute();
                Guid? r = default;
                Assert.True(select.Execute(out r));
                Assert.Equal(g, r);
            }
        }

        [Fact]
        public void GuidNull_Null()
        {
            using (var tbl = new TestTable("create table t (x text)"))
            using (var insert = tbl.Stmt<Guid?>("insert into t values (@x)"))
            using (var select = tbl.RStmt<Guid?>("select x from t"))
            {
                insert.Bind(null).Execute();
                Guid? r = Guid.NewGuid();
                Assert.True(select.Execute(out r));
                Assert.Null(r);
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        class Class<T> { public T Value { get; set; } = default!; }

        [Fact]
        public void Class_Default_Constructor()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (5)"))
            using (var select = tbl.RStmt<Class<int>>("select x from t"))
            {
                insert.Execute();
                Assert.True(select.Execute(out var r));
                Assert.Equal(expected: 5, r.Value);
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        class ClassWithConstructor<T, U>
        {
            public T Value { get; set; } = default!;
            public ClassWithConstructor(U required) =>
                throw new NotImplementedException();
        }

        [Fact]
        public void Class_NoDefaultConstructor_Throws()
        {
            using (var tbl = new TestTable("create table t (x int)"))
            using (var insert = tbl.Stmt("insert into t values (5)"))
            using (var select = tbl.RStmt<ClassWithConstructor<int, string>>("select x from t"))
            {
                insert.Execute();
                Assert.Throws<ArgumentException>(() =>
                    select.Execute(out _));
            }
        }
    }
}
