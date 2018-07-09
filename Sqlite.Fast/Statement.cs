using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sqlite.Fast
{
    public class Statement : IDisposable
    {
        private readonly IntPtr _statement;
        private readonly int _columnCount;

        private bool _disposed = false;

        internal Statement(IntPtr statement, int columnCount)
        {
            _statement = statement;
            _columnCount = columnCount;
        }

        public void Execute()
        {
            var rows = new Rows(_statement, _columnCount);
            foreach (var row in rows) { }
        }

        // ExecuteScalar?

        public Rows<TRecord> Execute<TRecord>(RowToRecordMap<TRecord> rowMap, CancellationToken ct = default)
        {
            if (rowMap.ColumnMaps.Length != _columnCount)
            {
                throw new ArgumentException($"Row-to-record map expects {rowMap.ColumnMaps.Length} columns; query returns {_columnCount} columns");
            }
            var rows = new Rows(_statement, _columnCount);
            return new Rows<TRecord>(rows, rowMap, ct);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        ~Statement() => Dispose();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Result r = Sqlite.Finalize(_statement);
            _disposed = true;
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to finalize prepared sql statement");
            }
        }
    }
}
