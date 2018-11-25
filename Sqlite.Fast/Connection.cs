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
            Result r = Sqlite.Open(dbFilePath, out IntPtr conn);
            if (r != Result.Ok)
            {
                Sqlite.CloseV2(conn);
                throw new SqliteException(r, "Failed to open database connection");
            }
            _connnection = conn;
        }

        public Statement CompileStatement(string sql)
        {
            CheckDisposed();
            Result r = Sqlite.PrepareV2(_connnection, sql, sqlByteCount: -1, out IntPtr stmt, out _);
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to compile sql statement");
            }
            int colCount = Sqlite.ColumCount(stmt);
            return new Statement(stmt, colCount);
        }

        public Statement<TRecord> CompileStatement<TRecord>(string sql, Converter<TRecord> converter)
        {
            return new Statement<TRecord>(CompileStatement(sql), converter);
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
            Result r = Sqlite.CloseV2(_connnection);
            _disposed = true;
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to close database connection");
            }
        }
    }
}