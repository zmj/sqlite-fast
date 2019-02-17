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
        /// The SQLite function.
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// The SQLite error code.
        /// </summary>
        public Sqlite.Result Error { get; }
        
        internal SqliteException(string function, Sqlite.Result error)
            : base($"{function}: {error}")
        {
            Function = function;
            Error = error;
        }
    }

    internal static class SqliteExceptionExtensions
    {
        public static void ThrowIfNotOK(this Sqlite.Result result, string function)
        {
            if (result != Sqlite.Result.Ok)
                throw new SqliteException(function, result);
        }

        public static void ThrowIfNot(this Sqlite.Result result, Sqlite.Result expected, string function)
        {
            if (result != expected)
                throw new SqliteException(function, result);
        }
    }
}
