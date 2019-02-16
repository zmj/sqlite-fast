using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// SqliteException is thrown when a SQLite call returns an unexpected error code.
    /// </summary>
    public class SqliteException : Exception
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public Sqlite.Result Error { get; }

        internal SqliteException(Sqlite.Result error, string message)
            : base($"{message} (SQLite error code {error})")
        {
            Error = error;
        }
    }

    internal static class SqliteExceptionExtensions
    {
        public static void ThrowIfNotOK(this Sqlite.Result result, string message)
        {
            if (result != Sqlite.Result.Ok)
                throw new SqliteException(result, message);
        }

        public static void ThrowIfNot(this Sqlite.Result result, Sqlite.Result expected, string message)
        {
            if (result != expected)
                throw new SqliteException(result, message);
        }
    }
}
