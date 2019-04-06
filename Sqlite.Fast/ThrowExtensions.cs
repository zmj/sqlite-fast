using System;

namespace Sqlite.Fast
{
    internal static class ThrowExtensions
    {
        public static void ThrowIfNull<T>(this T? value, string name)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void ThrowIfDisposed(this bool disposed, string name)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(name);
            }
        }

        public static void ThrowIfNotOK(this Sqlite.Result result, string function)
        {
            if (result != Sqlite.Result.Ok)
            {
                throw new SqliteException(function, result);
            }
        }

        public static void ThrowIfNot(this Sqlite.Result result, Sqlite.Result expected, string function)
        {
            if (result != expected)
            {
                throw new SqliteException(function, result);
            }
        }
    }
}