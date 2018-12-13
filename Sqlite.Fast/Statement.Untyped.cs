using System;
using System.Runtime.InteropServices;

namespace Sqlite.Fast
{
    public sealed class Statement : IDisposable
    {
        private readonly IntPtr _statement;
        internal readonly int ColumnCount;
        internal readonly int ParameterCount;
        private readonly VersionTokenSource _versionSource = new VersionTokenSource();

        private bool _needsReset = false;
        private bool _disposed = false;

        internal Statement(IntPtr statement)
        {
            _statement = statement;
            ColumnCount = Sqlite.ColumCount(statement);
            ParameterCount = Sqlite.BindParameterCount(statement);
        }

        public void Execute()
        {
            CheckDisposed();
            Rows rows = ExecuteInternal();
            rows.GetEnumerator().MoveNext();
        }

        internal Rows ExecuteInternal()
        {
            CheckDisposed();
            ResetIfNecessary();
            _needsReset = true;
            return new Rows(_statement, ColumnCount, _versionSource.Token);
        }

        private void ResetIfNecessary()
        {
            if (!_needsReset) return;
            Sqlite.Result r = Sqlite.Reset(_statement);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, "Failed to reset statement");
            }
            _needsReset = false;
            _versionSource.Version++;
        }

        internal void BindInteger(int index, long value)
        {
            CheckDisposed();
            ResetIfNecessary();
            Sqlite.Result r = Sqlite.BindInteger(_statement, index, value);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind {value} to parameter {index}");
            }
        }

        internal void BindFloat(double value)
        {
            CheckDisposed();
            ResetIfNecessary();

        }

        internal void BindText(int index, ReadOnlySpan<char> value)
        {
            CheckDisposed();
            ResetIfNecessary();
            Sqlite.Result r = Sqlite.BindText16(
                _statement,
                index,
                in MemoryMarshal.GetReference(value),
                value.Length << 1,
                new IntPtr(-1));
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind '{value.ToString()}' to parameter {index}");
            }
        }

        internal void BindBlob(int index, ReadOnlySpan<byte> value)
        {
            CheckDisposed();
            ResetIfNecessary();

        }

        internal void BindNull(int index)
        {
            CheckDisposed();
            ResetIfNecessary();

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
            Sqlite.Result r = Sqlite.Finalize(_statement);
            _disposed = true;
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, "Failed to finalize prepared sql statement");
            }
        }
    }
}
