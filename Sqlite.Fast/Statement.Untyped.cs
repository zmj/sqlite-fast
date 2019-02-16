﻿using System;
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
        private readonly StatementResetter _resetter;

        private StatementVersion? _lastExecuteVersion = null;
        private bool _disposed = false;

        internal Statement(IntPtr statement)
        {
            _statement = statement;
            ColumnCount = Sqlite.ColumnCount(statement);
            ParameterCount = Sqlite.BindParameterCount(statement);
            _resetter = new StatementResetter(statement);
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
            CheckDisposed();
            if (_lastExecuteVersion.HasValue)
            {
                _resetter.ResetIfNecessary(_lastExecuteVersion.Value);
            }
            _lastExecuteVersion = _resetter.CurrentVersion;
            return new Rows(_statement, ColumnCount, _resetter);
        }
        
        internal void BeginBinding()
        {
            CheckDisposed();
            if (_lastExecuteVersion.HasValue)
            {
                _resetter.ResetIfNecessary(_lastExecuteVersion.Value);
            }
        }

        internal void BindInteger(int index, long value)
        {
            Sqlite.BindInteger(_statement, index, value)
                .ThrowIfNotOK($"Failed to bind {value} to parameter {index}");
        }

        internal void BindFloat(int index, double value)
        {
            Sqlite.BindFloat(_statement, index, value)
                .ThrowIfNotOK($"Failed to bind {value} to parameter {index}");
        }

        internal void BindUtf16Text(int index, ReadOnlySpan<char> value)
        {
            Sqlite.BindText16(
                _statement,
                index,
                in MemoryMarshal.GetReference(value),
                value.Length << 1,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK($"Failed to bind '{value.ToString()}' to parameter {index}");
        }

        internal void BindUtf8Text(int index, ReadOnlySpan<byte> value)
        {
            Sqlite.BindText(
                _statement,
                index,
                in MemoryMarshal.GetReference(value),
                value.Length,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK($"Failed to bind UTF-8 text to parameter {index}"); // todo: tostring value
        }

        internal void BindBlob(int index, ReadOnlySpan<byte> value)
        {
            Sqlite.BindBlob(
                _statement,
                index,
                in MemoryMarshal.GetReference(value),
                value.Length,
                Sqlite.Destructor.Transient)
                .ThrowIfNotOK($"Failed to bind binary blob to parameter {index}");
        }

        internal void BindNull(int index)
        {
            Sqlite.BindNull(_statement, index)
                .ThrowIfNotOK($"Failed to bind null to parameter {index}");
        }               
    
        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
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
            r.ThrowIfNotOK("Failed to finalize prepared sql statement");
        }
    }
}
