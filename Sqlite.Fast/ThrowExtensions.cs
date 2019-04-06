using System;

internal static class ThrowExtensions
{
    public static void ThrowIfNull<T>(this T value, string name) // todo ?
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
}