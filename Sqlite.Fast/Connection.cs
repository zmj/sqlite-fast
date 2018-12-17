using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Connection : IDisposable
    {
        private readonly IntPtr _connnection;

        private bool _disposed = false;
        
        public Connection(string dbFilePath)
        {
            Sqlite.Result r = Sqlite.Open(dbFilePath, out IntPtr conn);
            _connnection = conn;
            if (r != Sqlite.Result.Ok)
            {
                Dispose();
                throw new SqliteException(r, "Failed to open database connection");
            }
        }

        public Statement CompileStatement(string sql)
        {
            CheckDisposed();
            Sqlite.Result r = Sqlite.PrepareV2(_connnection, sql, sqlByteCount: -1, out IntPtr stmt, out _);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, "Failed to compile sql statement");
            }
            return new Statement(stmt);
        }

        public Statement<TParams> CompileStatement<TParams>(string sql)
        {
            var converter = ParameterConverter.Default<TParams>();
            return CompileStatement(sql, converter);
        }

        public Statement<TParams> CompileStatement<TParams>(string sql, ParameterConverter<TParams> converter)
        {
            return new Statement<TParams>(CompileStatement(sql), converter);
        }
        
        public ResultStatement<TResult> CompileResultStatement<TResult>(string sql)
        {
            var converter = ResultConverter.Default<TResult>();
            return CompileStatement(sql, converter);
        }

        public ResultStatement<TResult> CompileStatement<TResult>(string sql, ResultConverter<TResult> converter)
        {
            return new ResultStatement<TResult>(CompileStatement(sql), converter);
        }

        public Statement<TParams, TResult> CompileStatement<TParams, TResult>(string sql)
        {
            var parameterConverter = ParameterConverter.Default<TParams>();
            var resultConverter = ResultConverter.Default<TResult>();
            return CompileStatement(sql, parameterConverter, resultConverter);
        }

        public Statement<TParams, TResult> CompileStatement<TParams, TResult>(
            string sql,
            ParameterConverter<TParams> parameterConverter,
            ResultConverter<TResult> resultConverter)
        {
            Statement statement = CompileStatement(sql);
            return new Statement<TParams, TResult>(
                new Statement<TParams>(statement, parameterConverter),
                new ResultStatement<TResult>(statement, resultConverter));
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        ~Connection() => Dispose(disposing: false);
        public void Dispose() => Dispose(disposing: true);

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            Sqlite.Result r = Sqlite.CloseV2(_connnection);
            _disposed = true;
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, "Failed to close database connection");
            }
        }
    }
}