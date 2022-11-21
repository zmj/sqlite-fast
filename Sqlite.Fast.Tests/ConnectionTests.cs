using Sqlite.Fast;
using SQLitePCL.Ugly;
using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void PathNulll()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Connection(null!));
        }

        [Fact]
        public void Path_Invalid()
        {
            Assert.Throws<ugly.sqlite3_exception>(
                () => new Connection(@"\\\///:"));
        }

        [Fact]
        public void Statement0_SqlNull()
        {
            using (var conn = new Connection())
            {
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement(null!));
            }
        }

        [Fact]
        public void Statement0_Disposed()
        {
            var conn = new Connection();
            conn.Dispose();
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement(null!));
        }


        [Fact]
        public void Statement1_SqlNull()
        {
            using (var conn = new Connection())
            {
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int>(null!));
            }
        }

        [Fact]
        public void Statement1_ConverterNull()
        {
            using (var conn = new Connection())
            {
                ParameterConverter<int> p = null!;
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement("", p));
            }
        }

        [Fact]
        public void Statement1_Disposed()
        {
            var conn = new Connection();
            conn.Dispose();
            ParameterConverter<int> p = null!;
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement(null!, p));
        }

        [Fact]
        public void Statement1R_SqlNull()
        {
            using (var conn = new Connection())
            {
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileResultStatement<int>(null!));
            }
        }

        [Fact]
        public void Statement1R_ConverterNull()
        {
            using (var conn = new Connection())
            {
                ResultConverter<int> r = null!;
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement("", r));
            }
        }

        [Fact]
        public void Statement1R_Disposed()
        {
            var conn = new Connection();
            conn.Dispose();
            ResultConverter<int> r = null!;
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement(null!, r));
        }

        [Fact]
        public void Statement2_SqlNull()
        {
            using (var conn = new Connection())
            {
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int, int>(null!));
            }
        }

        [Fact]
        public void Statement2_SqlNull_PConverter()
        {
            using (var conn = new Connection())
            {
                var p = ParameterConverter.Builder<int>().Compile();
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int, int>(null!, p));
            }
        }

        [Fact]
        public void Statement2_SqlNull_RConverter()
        {
            using (var conn = new Connection())
            {
                var r = ResultConverter.Builder<int>().Compile();
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int, int>(null!, r));
            }
        }

        [Fact]
        public void Statement2_SqlNull_BothConverters()
        {
            using (var conn = new Connection())
            {
                var p = ParameterConverter.Builder<int>().Compile();
                var r = ResultConverter.Builder<int>().Compile();
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement(null!, r, p));
            }
        }
        
        [Fact]
        public void Statement2_PConverterNull()
        {
            using (var conn = new Connection())
            {
                ParameterConverter<int> p = null!;
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int, int>("", p));
            }
        }

        [Fact]
        public void Statement2_PConverterNull_BothConverters()
        {
            using (var conn = new Connection())
            {
                ParameterConverter<int> p = null!;
                var r = ResultConverter.Builder<int>().Compile();
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement("", r, p));
            }
        }

        [Fact]
        public void Statement2_RConverterNull()
        {
            using (var conn = new Connection())
            {
                ResultConverter<int> r = null!;
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement<int, int>("", r));
            }
        }

        [Fact]
        public void Statement2_RConverterNull_BothConverters()
        {
            using (var conn = new Connection())
            {
                var p = ParameterConverter.Builder<int>().Compile();
                ResultConverter<int> r = null!;
                Assert.Throws<ArgumentNullException>(
                    () => conn.CompileStatement("", r, p));
            }
        }

        [Fact]
        public void Statement2_Disposed()
        {
            var conn = new Connection();
            conn.Dispose();
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement<int, int>(null!));
        }

        [Fact]
        public void Statement2_Disposed_PConverter()
        {
            var conn = new Connection();
            conn.Dispose();
            ParameterConverter<int> p = null!;
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement<int, int>(null!, p));
        }

        [Fact]
        public void Statement2_Disposed_RConverter()
        {
            var conn = new Connection();
            conn.Dispose();
            ResultConverter<int> r = null!;
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement<int, int>(null!, r));
        }

        [Fact]
        public void Statement2_Disposed_BothConverters()
        {
            var conn = new Connection();
            conn.Dispose();
            Assert.Throws<ObjectDisposedException>(
                () => conn.CompileStatement<int, int>(null!, null!, null!));
        }

        [Fact]
        public void DisposeTwice()
        {
            var conn = new Connection();
            conn.Dispose();
            conn.Dispose();
        }
    }
}