using System;
using System.Runtime.InteropServices;

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

        public static void ThrowIfClosed(this SafeHandle handle, string name)
        {
            if (handle.IsClosed)
            {
                throw new ObjectDisposedException(name);
            }
        }
    }
}