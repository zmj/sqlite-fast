using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class Connection : IDisposable
    {
        private readonly IntPtr _connnection;

        private bool _disposed = false;

        private Connection(IntPtr connnnection)
        {
            _connnection = connnnection;
        }

        public static Connection Open(string dbFilePath)
        {
            Result r = Sqlite.Open(dbFilePath, out IntPtr conn);
            if (r != Result.Ok)
            {
                Sqlite.CloseV2(conn);
                throw new SqliteException(r, "Failed to open database connection");
            }
            return new Connection(conn);
        }

        public Statement NewStatement(string sql)
        {
            CheckDisposed();
            Result r = Sqlite.PrepareV2(_connnection, sql, sqlByteCount: -1, out IntPtr stmt, out _);
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to prepare sql statement");
            }
            int colCount = Sqlite.ColumCount(stmt);
            return new Statement(stmt, colCount);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        ~Connection() => Dispose();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Result r = Sqlite.CloseV2(_connnection);
            _disposed = true;
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to close database connection");
            }
        }
    }
}