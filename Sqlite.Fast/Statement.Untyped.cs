using System;
using System.Runtime.InteropServices;

namespace Sqlite.Fast
{
    public sealed class Statement : IDisposable
    {
        private readonly IntPtr _statement;
        internal readonly int ColumnCount;
        private readonly int _bindCount;
        private readonly VersionTokenSource _versionSource = new VersionTokenSource();

        private bool _needsReset = false;
        private int _bindIndex = 1;
        private bool _disposed = false;

        internal Statement(IntPtr statement)
        {
            _statement = statement;
            ColumnCount = Sqlite.ColumCount(statement);
            _bindCount = Sqlite.BindParameterCount(statement);
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
            _bindIndex = 1;
        }
                
        public Statement Bind(long value)
        {
            CheckDisposed();
            ResetIfNecessary();
            if (_bindIndex > _bindCount) _bindIndex = 1;
            Sqlite.Result r = Sqlite.BindInteger(_statement, _bindIndex, value);
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind {value} to parameter {_bindIndex}");
            }
            _bindIndex++;
            return this;
        }

        public Statement Bind(string value) => Bind(value.AsSpan());
        
        public Statement Bind(ReadOnlySpan<char> value)
        {
            CheckDisposed();
            ResetIfNecessary();
            if (_bindIndex > _bindCount) _bindIndex = 1;
            Sqlite.Result r = Sqlite.BindText16(
                _statement, 
                _bindIndex,
                in MemoryMarshal.GetReference(value), 
                value.Length << 1, 
                new IntPtr(-1));
            if (r != Sqlite.Result.Ok)
            {
                throw new SqliteException(r, $"Failed to bind '{value.ToString()}' to parameter {_bindIndex}");
            }
            _bindIndex++;
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
