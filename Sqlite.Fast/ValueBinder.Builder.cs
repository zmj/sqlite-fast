using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate long ToInteger<T>(T value);
    public delegate double ToFloat<T>(T value);
    public delegate void ToText<T>(T value, Span<char> destination);
    public delegate ReadOnlySpan<char> AsText<T>(T value);
    internal delegate void ToUtf8Text<T>(T value, Span<byte> destination);
    internal delegate ReadOnlySpan<byte> AsUtf8Text<T>(T value);
    public delegate void ToBlob<T>(T value, Span<byte> destination);
    public delegate ReadOnlySpan<byte> AsBlob<T>(T value);
    public delegate void ToNull<T>(T value);

    internal delegate TField FieldGetter<TParams, TField>(in TParams parameters);

    internal static class ValueBinder
    {
        public static IBuilder<TParams> Build<TParams>(MemberInfo member) =>
            (IBuilder<TParams>)Buildpublic<TParams>(member);

        public static Builder<TParams, TParams> Build<TParams>() =>
            (Builder<TParams, TParams>)Buildpublic<TParams>(member: null);

        private static object Buildpublic<TParams>(MemberInfo member)
        {
            Type valueType = member != null ? member.ValueType() : typeof(TParams);
            ConstructorInfo constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TParams), valueType })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return constructor.Invoke(new[] { member });
        }

        public interface IBuilder<TParams>
        {
            MemberInfo Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        public sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo Member { get; }
            public readonly List<Converter<TField>> Converters = new List<Converter<TField>>();

            public Builder(MemberInfo member) => Member = member;

            IValueBinder<TParams> IBuilder<TParams>.Compile(bool withDefaults) => Compile(withDefaults);

            public ValueBinder<TParams, TField> Compile(bool withDefaults)
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

        public readonly struct Converter<TField>
        {
            public readonly Sqlite.DataType DataType;
            public readonly bool Utf8Text;
            public readonly ToInteger<TField> ToInteger;
            public readonly ToFloat<TField> ToFloat;
            public readonly ToText<TField> ToUtf16Text;
            public readonly AsText<TField> AsUtf16Text;
            public readonly ToUtf8Text<TField> ToUtf8Text;
            public readonly AsUtf8Text<TField> AsUtf8Text;
            public readonly ToBlob<TField> ToBlob;
            public readonly AsBlob<TField> AsBlob;
            public readonly Func<TField, int> Length;

            private readonly Func<TField, bool> _canConvert;

            public Converter(
                Sqlite.DataType dataType,
                bool utf8Text,
                Func<TField, bool> canConvert,
                Func<TField, int> length,
                ToInteger<TField> toInteger,
                ToFloat<TField> toFloat,
                ToText<TField> toUtf16Text,
                AsText<TField> asUtf16Text,
                ToUtf8Text<TField> toUtf8Text,
                AsUtf8Text<TField> asUtf8Text,
                ToBlob<TField> toBlob,
                AsBlob<TField> asBlob)
            {
                DataType = dataType;
                Utf8Text = utf8Text;
                _canConvert = canConvert;
                Length = length;
                ToInteger = toInteger;
                ToFloat = toFloat;
                ToUtf16Text = toUtf16Text;
                AsUtf16Text = asUtf16Text;
                ToUtf8Text = toUtf8Text;
                AsUtf8Text = asUtf8Text;
                ToBlob = toBlob;
                AsBlob = asBlob;
            }

            public bool CanConvert(TField value) => _canConvert == null || _canConvert(value);
        }

        public static class Converter
        { 
            public static Converter<T> Integer<T>(ToInteger<T> toInteger) => Integer(canConvert: null, toInteger);
            public static Converter<T> Integer<T>(Func<T, bool> canConvert, ToInteger<T> toInteger) =>
                new Converter<T>(
                    Sqlite.DataType.Integer,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Float<T>(ToFloat<T> toFloat) => Float(canConvert: null, toFloat);
            public static Converter<T> Float<T>(Func<T, bool> canConvert, ToFloat<T> toFloat) =>
                new Converter<T>(
                    Sqlite.DataType.Float,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf8Text<T>(ToUtf8Text<T> toUtf8Text, Func<T, int> byteLength) =>
                Utf8Text(canConvert: null, toUtf8Text, byteLength);
            public static Converter<T> Utf8Text<T>(Func<T, bool> canConvert, ToUtf8Text<T> toUtf8Text, Func<T, int> byteLength) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    byteLength,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf8Text<T>(AsUtf8Text<T> asUtf8Text) => Utf8Text(canConvert: null, asUtf8Text);
            public static Converter<T> Utf8Text<T>(Func<T, bool> canConvert, AsUtf8Text<T> asUtf8Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf16Text<T>(ToText<T> toUtf16Text, Func<T, int> utf16Length) =>
                Utf16Text(canConvert: null, toUtf16Text, utf16Length);
            public static Converter<T> Utf16Text<T>(Func<T, bool> canConvert, ToText<T> toUtf16Text, Func<T, int> utf16Length) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    utf16Length,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf16Text<T>(AsText<T> asUtf16Text) => Utf16Text(canConvert: null, asUtf16Text);
            public static Converter<T> Utf16Text<T>(Func<T, bool> canConvert, AsText<T> asUtf16Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Blob<T>(ToBlob<T> toBlob, Func<T, int> byteLength) => Blob(canConvert: null, toBlob, byteLength);
            public static Converter<T> Blob<T>(Func<T, bool> canConvert, ToBlob<T> toBlob, Func<T, int> byteLength) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    byteLength,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob,
                    asBlob: null);

            public static Converter<T> Blob<T>(AsBlob<T> asBlob) => Blob(canConvert: null, asBlob);
            public static Converter<T> Blob<T>(Func<T, bool> canConvert, AsBlob<T> asBlob) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob);

            public static Converter<T> Null<T>() => Null<T>(canConvert: null);
            public static Converter<T> Null<T>(Func<T, bool> canConvert) =>
                new Converter<T>(
                    Sqlite.DataType.Null,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);
        }
    }
}
