using System;
using System.Runtime.InteropServices;

namespace Sqlite.Fast
{
    /// <summary>
    /// Statement wraps a SQLite prepared statement that has no paremeters and no results.
    /// Create a Statement (by calling Connection.CompileStatement), reuse it as many times as necessary, then dispose it.
    /// Statement instances are not thread-safe.
    /// </summary>
    public sealed class Statement : IDisposable
    {
        private readonly IntPtr _statement;
        internal readonly int ColumnCount;
        internal readonly int ParameterCount;
        
        private bool _disposed = false;

        internal Statement(IntPtr statement)
        {
            _statement = statement;
            ColumnCount = Sqlite.ColumnCount(statement);
            ParameterCount = Sqlite.BindParameterCount(statement);
        }

        /// <summary>
        /// Executes the statement.
        /// </summary>
        public void Execute()
        {
            Rows.Enumerator rows = ExecuteInternal().GetEnumerator();
            try
            {
                rows.MoveNext();
            }
            finally
            {
                rows.Dispose();
            }
        }

        internal Rows ExecuteInternal()
        {
            _disposed.ThrowIfDisposed(nameof(Statement));
            return new Rows(_statement, ColumnCount);
        }

        internal void BeginBinding() => _disposed.ThrowIfDisposed(nameof(Statement));

        internal void BindInteger(int index, long value)
        {
            Sqlite.BindInt64(_statement, index, value)
                .ThrowIfNotOK(nameof(Sqlite.BindInt64));
        }

        internal void BindFloat(int index, double value)
        {
            Sqlite.BindDouble(_statement, index, value)
                .ThrowIfNotOK(nameof(Sqlite.BindDouble));
        }

        internal void BindUtf16Text(int index, in ReadOnlySpan<char> value)
        {
            Sqlite.BindText16(
                _statement,
                index,
                ref MemoryMarshal.GetReference(value),
                value.Length << 1,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK(nameof(Sqlite.BindText16));
        }

        internal void BindUtf8Text(int index, in ReadOnlySpan<byte> value)
        {
            Sqlite.BindText(
                _statement,
                index,
                ref MemoryMarshal.GetReference(value),
                value.Length,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK(nameof(Sqlite.BindText));
        }

        internal void BindBlob(int index, in ReadOnlySpan<byte> value)
        {
            Sqlite.BindBlob(
                _statement,
                index,
                ref MemoryMarshal.GetReference(value),
                value.Length,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK(nameof(Sqlite.BindBlob));
        }

        internal void BindNull(int index)
        {
            Sqlite.BindNull(_statement, index)
                .ThrowIfNotOK(nameof(Sqlite.BindNull));
        }

        /// <summary>
        /// Finalizes the prepared statement. If Dispose is not called, the prepared statement will be finalized by the finalizer thread.
        /// </summary>
        public void Dispose() => Dispose(disposing: true);

        /// <summary>
        /// Finalizes the prepared statement.
        /// </summary>
        ~Statement() => Dispose(disposing: false);

        private void Dispose(bool disposing)
        { 
            if (_disposed)
            {
                return;
            }
            Sqlite.Result r = Sqlite.Finalize(_statement);
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            _disposed = true;
            r.ThrowIfNotOK(nameof(Sqlite.Finalize));
        }
    }
}
