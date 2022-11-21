using SQLitePCL;
using SQLitePCL.Ugly;
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
        private readonly sqlite3_stmt _statement;
        internal readonly int ColumnCount;
        internal readonly int ParameterCount;

        internal Statement(sqlite3_stmt statement)
        {
            _statement = statement;
            ColumnCount = statement.column_count();
            ParameterCount = statement.bind_parameter_count();
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
            _statement.ThrowIfClosed(nameof(Statement));
            return new Rows(_statement);
        }

        internal void BeginBinding() => _statement.ThrowIfClosed(nameof(Statement));

        internal void BindInteger(int index, long value) => _statement.bind_int64(index, value);

        internal void BindFloat(int index, double value) => _statement.bind_double(index, value);

        internal void BindUtf16Text(int index, ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                BindEmptyString(index);
            }
            else
            {
                _statement.bind_text16(index, value);
            }
        }

        internal void BindUtf8Text(int index, ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
            {
                BindEmptyString(index);
            }
            else
            {
                _statement.bind_text(index, value);
            }
        }

        private void BindEmptyString(int index)
        {
            Span<byte> terminator = stackalloc byte[1];
            _statement.bind_text(index, utf8z.FromSpan(terminator));
        }

        internal void BindBlob(int index, ReadOnlySpan<byte> value) => _statement.bind_blob(index, value);

        internal void BindNull(int index) => _statement.bind_null(index);

        /// <summary>
        /// Finalizes the prepared statement. If Dispose is not called, the prepared statement will be finalized by the finalizer thread.
        /// </summary>
        public void Dispose() => _statement.Dispose();
    }
}
