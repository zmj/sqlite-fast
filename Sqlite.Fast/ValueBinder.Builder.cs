using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate long ToInteger<T>(T value);
    public delegate double ToFloat<T>(T value);
    public delegate ReadOnlySpan<char> ToText<T>(T value);
    internal delegate ReadOnlySpan<byte> ToUtf8Text<T>(T value);
    public delegate ReadOnlySpan<byte> ToBlob<T>(T value);

    internal delegate TField FieldGetter<TParams, TField>(in TParams parameters);

    internal static class ValueBinder
    {
        internal interface IBuilder<TParams>
        {
            MemberInfo Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo Member { get; }

            public IValueBinder<TParams> Compile(bool withDefaults)
            {
                throw new NotImplementedException();
            }

            public Builder<TParams, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TParams, TCallerField> builder)
                {
                    return builder;
                }
                throw new InvalidOperationException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }
    }
}
