using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate long ToInteger<T>(T value);
    public delegate double ToFloat<T>(T value);
    public delegate ReadOnlySpan<char> ToText<T>(T value);
    internal delegate ReadOnlySpan<byte> ToUtf8Text<T>(T value);
    public delegate ReadOnlySpan<byte> ToBlob<T>(T value);
    public delegate void ToNull<T>(T value);

    internal delegate TField FieldGetter<TParams, TField>(in TParams parameters);

    internal static class ValueBinder
    {
        internal static IBuilder<TParams> Build<TParams>(MemberInfo member) =>
            (IBuilder<TParams>)BuildInternal<TParams>(member);

        internal static Builder<TParams, TParams> Build<TParams>() =>
            (Builder<TParams, TParams>)BuildInternal<TParams>(member: null);

        private static object BuildInternal<TParams>(MemberInfo member)
        {
            Type valueType = member != null ? member.ValueType() : typeof(TParams);
            ConstructorInfo constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TParams), valueType })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return constructor.Invoke(new[] { member });
        }

        internal interface IBuilder<TParams>
        {
            MemberInfo Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo Member { get; }
            internal readonly List<Converter<TField>> Converters = new List<Converter<TField>>();

            public Builder(MemberInfo member) => Member = member;

            IValueBinder<TParams> IBuilder<TParams>.Compile(bool withDefaults) => Compile(withDefaults);

            internal ValueBinder<TParams, TField> Compile(bool withDefaults)
            {
                IEnumerable<Converter<TField>> converters;
                if (withDefaults)
                {
                    Converter<TField>[] defaultConverters = DefaultConverters.To<TField>();
                    if (Converters.Count > 0)
                    {
                        Converters.AddRange(defaultConverters);
                        converters = Converters;
                    }
                    else
                    {
                        converters = defaultConverters;
                    }
                }
                else
                {
                    converters = Converters;
                }
                return new ValueBinder<TParams, TField>(
                    Member?.Name,
                    CompileGetter(Member),
                    converters);
            }

            private static FieldGetter<TParams, TField> CompileGetter(MemberInfo member)
            {
                var parameters = Expression.Parameter(typeof(TParams).MakeByRefType());
                Expression value;
                if (member == null)
                {
                    value = parameters;
                }
                else
                {
                    value = Expression.MakeMemberAccess(parameters, member);
                }
                var lambda = Expression.Lambda<FieldGetter<TParams, TField>>(value, parameters);
                return lambda.Compile();
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

        internal readonly struct Converter<TField>
        {
            internal readonly Sqlite.DataType DataType;
            internal readonly bool Utf8Text;
            internal readonly ToInteger<TField> ToInteger;
            internal readonly ToFloat<TField> ToFloat;
            internal readonly ToText<TField> ToUtf16Text;
            internal readonly ToUtf8Text<TField> ToUtf8Text;
            internal readonly ToBlob<TField> ToBlob;

            private readonly Func<TField, bool> _canConvert;

            internal Converter(
                Sqlite.DataType dataType,
                bool utf8Text,
                Func<TField, bool> canConvert,
                ToInteger<TField> toInteger,
                ToFloat<TField> toFloat,
                ToText<TField> toUtf16Text,
                ToUtf8Text<TField> toUtf8Text,
                ToBlob<TField> toBlob)
            {
                DataType = dataType;
                Utf8Text = utf8Text;
                _canConvert = canConvert;
                ToInteger = toInteger;
                ToFloat = toFloat;
                ToUtf16Text = toUtf16Text;
                ToUtf8Text = toUtf8Text;
                ToBlob = toBlob;
            }

            internal bool CanConvert(TField value) => _canConvert == null || _canConvert(value);
        }

        internal static class Converter
        { 
            internal static Converter<T> Integer<T>(ToInteger<T> toInteger) => Integer(canConvert: null, toInteger);
            internal static Converter<T> Integer<T>(Func<T, bool> canConvert, ToInteger<T> toInteger) =>
                new Converter<T>(
                    Sqlite.DataType.Integer,
                    utf8Text: false,
                    canConvert,
                    toInteger,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);

            internal static Converter<T> Float<T>(ToFloat<T> toFloat) => Float(canConvert: null, toFloat);
            internal static Converter<T> Float<T>(Func<T, bool> canConvert, ToFloat<T> toFloat) =>
                new Converter<T>(
                    Sqlite.DataType.Float,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);

            internal static Converter<T> Utf8Text<T>(ToUtf8Text<T> toUtf8Text) => Utf8Text(canConvert: null, toUtf8Text);
            internal static Converter<T> Utf8Text<T>(Func<T, bool> canConvert, ToUtf8Text<T> toUtf8Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text,
                    toBlob: null);

            internal static Converter<T> Utf16Text<T>(ToText<T> toUtf16Text) => Utf16Text(canConvert: null, toUtf16Text);
            internal static Converter<T> Utf16Text<T>(Func<T, bool> canConvert, ToText<T> toUtf16Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text,
                    toUtf8Text: null,
                    toBlob: null);

            internal static Converter<T> Blob<T>(ToBlob<T> toBlob) => Blob(canConvert: null, toBlob);
            internal static Converter<T> Blob<T>(Func<T, bool> canConvert, ToBlob<T> toBlob) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob);

            internal static Converter<T> Null<T>() => Null<T>(canConvert: null);
            internal static Converter<T> Null<T>(Func<T, bool> canConvert) =>
                new Converter<T>(
                    Sqlite.DataType.Null,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);
        }
    }
}
