using System;

internal static class ThrowExtensions
{
#nullable enable
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
#nullable restore
}