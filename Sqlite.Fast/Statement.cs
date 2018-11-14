using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Sqlite.Fast
{
    public sealed class Statement : IDisposable
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
            CheckDisposed();
            var rows = new Rows(_statement, _columnCount);
            foreach (var row in rows) { }
        }

        public bool Execute<TRecord>(RowToRecordMap<TRecord> rowMap, ref TRecord record)
        {
            CheckDisposed();
            var rows = Execute(rowMap);
            bool assigned = false;
            foreach (var row in rows)
            {
                row.AssignTo(ref record);
                assigned = true;
            }
            return assigned;
        }

        public Rows<TRecord> Execute<TRecord>(RowToRecordMap<TRecord> rowMap, CancellationToken ct = default)
        {
            CheckDisposed();
            if (rowMap.ColumnMaps.Length != _columnCount)
            {
                throw new ArgumentException($"Row-to-record map expects {rowMap.ColumnMaps.Length} columns; query returns {_columnCount} columns");
            }
            var rows = new Rows(_statement, _columnCount);
            return new Rows<TRecord>(rows, rowMap, ct);
        }
        
        private Statement BindInteger(int parameterIndex, long parameterValue)
        {
            CheckDisposed();
            Result r = Sqlite.BindInteger(_statement, parameterIndex + 1, parameterValue);
            if (r != Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind {parameterValue} to parameter {parameterIndex}");
            }
            return this;
        }

        public Statement Bind(int parameterIndex, long parameterValue) => BindInteger(parameterIndex, parameterValue);
        public Statement Bind(int parameterIndex, ulong parameterValue) => BindInteger(parameterIndex, (long)parameterValue);
        public Statement Bind(int parameterIndex, int parameterValue) => BindInteger(parameterIndex, parameterValue);
        public Statement Bind(int parameterIndex, uint parameterValue) => BindInteger(parameterIndex, parameterValue);

        public Statement Bind(int parameterIndex, string parameterValue)
            => BindText(parameterIndex, parameterValue.AsSpan());
        public Statement Bind(int parameterIndex, ReadOnlySpan<char> parameterValue)
            => BindText(parameterIndex, parameterValue);

        private Statement BindText(int parameterIndex, ReadOnlySpan<char> parameterValue)
        {
            CheckDisposed();
            Result r = Sqlite.BindText16(
                _statement, 
                parameterIndex + 1,
                in MemoryMarshal.GetReference(parameterValue), 
                parameterValue.Length << 1, 
                new IntPtr(-1));
            if (r != Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind '{parameterValue.ToString()}' to parameter {parameterIndex}");
            }
            return this;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        ~Statement() => Dispose(disposing: false);
        public void Dispose() => Dispose(disposing: true);

        private void Dispose(bool disposing)
        { 
            if (_disposed)
            {
                return;
            }
            Result r = Sqlite.Finalize(_statement);
            _disposed = true;
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            if (r != Result.Ok)
            {
                throw new SqliteException(r, "Failed to finalize prepared sql statement");
            }
        }
    }
}
