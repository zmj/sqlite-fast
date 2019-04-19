using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Statement wraps a SQLite prepared statement that has one or more result rows.
    /// Create a Statement (by calling Connection.CompileStatement), reuse it as many times as necessary, then dispose it.
    /// Statement instances are not thread-safe.
    /// </summary>
    public sealed class ResultStatement<TResult> : IDisposable
    {
        private readonly Statement _statement;
        private readonly ResultConverter<TResult> _converter;

        internal ResultStatement(Statement statement, ResultConverter<TResult> converter)
        {
            try
            {
                _statement = statement;
                _converter = converter;
                ValidateConverter(converter);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Executes the statement and assigns the first result row to an instance of the result type.
        /// </summary>
        /// <returns>False if there were no result rows; true otherwise.</returns>
        public bool Execute(out TResult result)
        {
            Rows<TResult>.Enumerator rows = ExecuteInternal(_converter).GetEnumerator();
            try
            {
                if (!rows.MoveNext())
                {
                    result = default!;
                    return false;
                }
                rows.Current.AssignTo(out result);
                return true;
            }
            finally
            {
                rows.Dispose();
            }
        }

        /// <summary>
        /// Executes the statement and assigns the first result row using a custom converter.
        /// </summary>
        /// <returns>False if there were no result rows; true otherwise.</returns>
        public bool Execute<TCallerResult>(ResultConverter<TCallerResult> converter, out TCallerResult result)
        {
            ValidateConverter(converter);
            Rows<TCallerResult>.Enumerator rows = ExecuteInternal(converter).GetEnumerator();
            try
            {
                if (!rows.MoveNext())
                {
                    result = default!;
                    return false;
                }
                rows.Current.AssignTo(out result);
                return true;
            }
            finally
            {
                rows.Dispose();
            }
        }

        /// <summary>
        /// Executes the statement.
        /// </summary>
        /// <returns>An enumeration of result rows.</returns>
        public Rows<TResult> Execute() => ExecuteInternal(_converter);

        /// <summary>
        /// Executes the statement and assigns the result rows using a custom converter.
        /// </summary>
        /// <returns>An enueration of result rows.</returns>
        public Rows<TCallerResult> Execute<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            ValidateConverter(converter);
            return ExecuteInternal(converter);
        }

        private Rows<TCallerResult> ExecuteInternal<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            Rows rows = _statement.ExecuteInternal();
            return new Rows<TCallerResult>(rows, converter);
        }

        private void ValidateConverter<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            converter.ThrowIfNull(nameof(ResultConverter));
            if (converter.FieldCount != _statement.ColumnCount)
            {
                throw new ArgumentException($"Converter expects {converter.FieldCount} columns; statement returns {_statement.ColumnCount} columns");
            }
        }
        
        /// <summary>
        /// Finalizes the prepared statement. If Dispose is not called, the prepared statement will be finalized by the finalizer thread.
        /// </summary>
        public void Dispose() => _statement.Dispose();
    }
}
